
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PackageCliTool.Configuration;
using PackageCliTool.Logging;
using PackageCliTool.Models;
using PackageCliTool.Services;
using PackageCliTool.UI;
using PackageCliTool.Workflows;
using PSW.Shared.Configuration;
using PSW.Shared.Services;
using Spectre.Console;
using System.Net.Sockets;

namespace PackageCliTool;

/// <summary>
/// Main program class for the Package Script Writer CLI tool
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        ILogger? logger = null;

        try
        {
            // Parse command-line arguments
            var options = CommandLineOptions.Parse(args);

            // Initialize logging (verbose mode based on environment or flag)
            var verboseMode = Environment.GetEnvironmentVariable("PSW_VERBOSE") == "1" || options.VerboseMode;
            LoggerSetup.Initialize(verboseMode, enableFileLogging: true);
            logger = LoggerSetup.CreateLogger("Program");

            logger.LogInformation("PSW CLI started with {ArgCount} arguments", args.Length);

            // -----------------------------
            // CONFIGURATION: load appsettings next to the exe/DLL
            // -----------------------------
            var baseDir = AppContext.BaseDirectory;
            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(baseDir)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            // -----------------------------
            // DI container + Options binding
            // -----------------------------
            var services = new ServiceCollection();

            // Make IConfiguration available if needed elsewhere
            services.AddSingleton<IConfiguration>(configuration);

            services.AddMemoryCache();

            // Configure HttpClient with IPv4-only handler to avoid IPv6 timeout issues
            services.AddHttpClient(Options.DefaultName, client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                ConnectCallback = async (context, cancellationToken) =>
                {
                    // Force IPv4 to avoid ~42 second IPv6 timeout
                    var socket = new Socket(System.Net.Sockets.AddressFamily.InterNetwork,
                        System.Net.Sockets.SocketType.Stream,
                        System.Net.Sockets.ProtocolType.Tcp)
                    {
                        NoDelay = true
                    };

                    try
                    {
                        await socket.ConnectAsync(context.DnsEndPoint, cancellationToken);
                        return new System.Net.Sockets.NetworkStream(socket, ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                },
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2)
            });

            services.Configure<PSWConfig>(
                configuration.GetSection(PSWConfig.SectionName));

            // Register all services using proper DI
            ConfigureServices(services, logger);

            // Build the service provider with proper disposal
            using var serviceProvider = services.BuildServiceProvider();

            // Resolve services from DI container
            var scriptGeneratorService = serviceProvider.GetRequiredService<IScriptGeneratorService>();
            var cacheService = serviceProvider.GetRequiredService<CacheService>();
            var apiClient = serviceProvider.GetRequiredService<ApiClient>();
            var packageSelector = serviceProvider.GetRequiredService<PackageSelector>();
            var scriptExecutor = serviceProvider.GetRequiredService<ScriptExecutor>();
            var templateService = serviceProvider.GetRequiredService<TemplateService>();
            var historyService = serviceProvider.GetRequiredService<HistoryService>();
            var versionCheckService = serviceProvider.GetRequiredService<VersionCheckService>();

            // Handle help flag
            if (options.ShowHelp)
            {
                ConsoleDisplay.DisplayHelp();
                return;
            }

            // Handle version flag
            if (options.ShowVersion)
            {
                ConsoleDisplay.DisplayVersion();
                return;
            }

            // Handle clear cache flag
            if (options.ClearCache)
            {
                cacheService.Clear();
                AnsiConsole.MarkupLine("[green]✓ Cache cleared successfully[/]");

                // If only clearing cache, exit
                if (!options.HasAnyOptions() && !options.IsTemplateCommand() && !options.IsHistoryCommand())
                {
                    return;
                }
            }

            // Check if this is a versions command
            if (options.IsVersionsCommand())
            {
                var versionsWorkflow = new VersionsWorkflow(serviceProvider.GetRequiredService<IOptions<PSWConfig>>(), logger);
                await versionsWorkflow.RunAsync(options);
            }
            // Check if this is a history command
            else if (options.IsHistoryCommand())
            {
                var historyWorkflow = new HistoryWorkflow(historyService, scriptExecutor, scriptGeneratorService, logger);
                await historyWorkflow.RunAsync(options);
            }
            // Check if this is a template command
            else if (options.IsTemplateCommand())
            {
                var templateWorkflow = new TemplateWorkflow(templateService, scriptExecutor, scriptGeneratorService, historyService, logger);
                await templateWorkflow.RunAsync(options);
            }
            // Determine if we should use CLI mode or interactive mode
            else if (options.HasAnyOptions())
            {
                var communityTemplateService = serviceProvider.GetRequiredService<CommunityTemplateService>();
                var cliWorkflow = new CliModeWorkflow(apiClient, scriptExecutor, scriptGeneratorService, historyService, communityTemplateService, logger);
                await cliWorkflow.RunAsync(options);
            }
            else
            {
                // Interactive mode with Ctrl+C restart support
                var keepRunning = true;
                while (keepRunning)
                {
                    try
                    {
                        var pswConfig = serviceProvider.GetRequiredService<IOptions<PSWConfig>>().Value;
                        var interactiveWorkflow = new InteractiveModeWorkflow(
                            apiClient,
                            packageSelector,
                            scriptExecutor,
                            scriptGeneratorService,
                            versionCheckService,
                            historyService,
                            pswConfig,
                            logger);
                        await interactiveWorkflow.RunAsync();
                        keepRunning = false; // Exit loop on normal completion
                    }
                    catch (OperationCanceledException)
                    {
                        // User pressed Ctrl+C - restart the workflow
                        logger?.LogInformation("User pressed Ctrl+C, restarting interactive mode");
                        AnsiConsole.Clear();
                        AnsiConsole.MarkupLine("\n[yellow]↻ Restarting...[/]\n");
                        await Task.Delay(500); // Brief pause before restart
                    }
                }
            }
            logger?.LogInformation("PSW CLI completed successfully");
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, logger, showStackTrace: logger != null);
            Environment.ExitCode = 1;
        }
        finally
        {
            LoggerSetup.Shutdown();
        }
    }

    /// <summary>
    /// Configures all services for dependency injection
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <param name="logger">Optional logger instance for service initialization</param>
    private static void ConfigureServices(IServiceCollection services, ILogger? logger)
    {
        // Register core services with interfaces (Singleton for console app)
        services.AddSingleton<IScriptGeneratorService, ScriptGeneratorService>();
        services.AddSingleton<IPackageService, MarketplacePackageService>();
        services.AddSingleton<IUmbracoVersionService, UmbracoVersionService>();

        // Register CacheService with factory pattern for logger injection
        services.AddSingleton<CacheService>(sp =>
        {
            var cacheLogger = LoggerSetup.CreateLogger("CacheService");
            return new CacheService(ttlHours: 1, enabled: true, logger: cacheLogger);
        });

        // Register ApiClient with dependencies
        services.AddSingleton<ApiClient>(sp =>
        {
            var apiLogger = LoggerSetup.CreateLogger("ApiClient");
            var cacheService = sp.GetRequiredService<CacheService>();
            var packageService = sp.GetRequiredService<IPackageService>();
            return new ApiClient(ApiConfiguration.ApiBaseUrl, apiLogger, cacheService, packageService);
        });

        // Register PackageSelector with dependencies
        services.AddSingleton<PackageSelector>(sp =>
        {
            var apiClient = sp.GetRequiredService<ApiClient>();
            var packageService = sp.GetRequiredService<IPackageService>();
            var memoryCache = sp.GetRequiredService<IMemoryCache>();
            var packageLogger = LoggerSetup.CreateLogger("PackageSelector");
            return new PackageSelector(apiClient, packageService, memoryCache, packageLogger);
        });

        // Register ScriptExecutor with logger
        services.AddSingleton<ScriptExecutor>(sp =>
        {
            var scriptLogger = LoggerSetup.CreateLogger("ScriptExecutor");
            return new ScriptExecutor(scriptLogger);
        });

        // Register TemplateService with logger
        services.AddSingleton<TemplateService>(sp =>
        {
            var templateLogger = LoggerSetup.CreateLogger("TemplateService");
            return new TemplateService(logger: templateLogger);
        });

        // Register CommunityTemplateService with dependencies
        services.AddSingleton<CommunityTemplateService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var cacheService = sp.GetRequiredService<CacheService>();
            var communityLogger = LoggerSetup.CreateLogger("CommunityTemplateService");
            return new CommunityTemplateService(
                httpClient: httpClientFactory.CreateClient(),
                cacheService: cacheService,
                logger: communityLogger);
        });

        // Register HistoryService with logger
        services.AddSingleton<HistoryService>(sp =>
        {
            var historyLogger = LoggerSetup.CreateLogger("HistoryService");
            return new HistoryService(logger: historyLogger);
        });

        // Register VersionCheckService with dependencies
        services.AddSingleton<VersionCheckService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var versionLogger = LoggerSetup.CreateLogger("VersionCheckService");
            return new VersionCheckService(httpClientFactory.CreateClient(), versionLogger);
        });
    }
}
