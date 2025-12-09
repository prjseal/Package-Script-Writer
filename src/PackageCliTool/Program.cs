
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

            // Your services (register as before)
            services.AddScoped<IScriptGeneratorService, ScriptGeneratorService>();
            services.AddScoped<IPackageService, MarketplacePackageService>();
            services.AddScoped<IQueryStringService, QueryStringService>();
            services.AddScoped<IUmbracoVersionService, UmbracoVersionService>();

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            var scriptGeneratorService = serviceProvider.GetRequiredService<IScriptGeneratorService>();
            var packageService = serviceProvider.GetRequiredService<IPackageService>();
            var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();

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

            // Initialize cache service (1-hour TTL, disabled if --no-cache is set)
            var cacheService = new CacheService(ttlHours: 1, enabled: !options.NoCache, logger: logger);

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

            // Handle update packages flag
            if (options.UpdatePackageCache)
            {
                AnsiConsole.MarkupLine("[yellow]Updating package cache from PSW API...[/]");
                var tempPackageSelector = new PackageSelector(
                    new ApiClient(ApiConfiguration.ApiBaseUrl, logger, cacheService),
                    packageService,
                    memoryCache,
                    logger);
                await tempPackageSelector.PopulateAllPackagesAsync(forceUpdate: true);
                AnsiConsole.MarkupLine("[green]✓ Package cache updated successfully[/]");

                // If only updating cache, exit
                if (!options.HasAnyOptions() && !options.IsTemplateCommand() && !options.IsHistoryCommand())
                {
                    return;
                }
            }

            // Initialize services that depend on configuration
            var apiClient = new ApiClient(ApiConfiguration.ApiBaseUrl, logger, cacheService);
            var packageSelector = new PackageSelector(apiClient, packageService, memoryCache, logger);
            var scriptExecutor = new ScriptExecutor(logger);
            var templateService = new TemplateService(logger: logger);
            var historyService = new HistoryService(logger: logger);
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var versionCheckService = new VersionCheckService(httpClientFactory.CreateClient(), logger);

            // Check if this is a history command
            if (options.IsHistoryCommand())
            {
                var historyWorkflow = new HistoryWorkflow(historyService, scriptExecutor, scriptGeneratorService, logger);
                await historyWorkflow.RunAsync(options);
            }
            // Check if this is a template command
            else if (options.IsTemplateCommand())
            {
                var templateWorkflow = new TemplateWorkflow(templateService, scriptExecutor, scriptGeneratorService, logger);
                await templateWorkflow.RunAsync(options);
            }
            // Determine if we should use CLI mode or interactive mode
            else if (options.HasAnyOptions())
            {
                var cliWorkflow = new CliModeWorkflow(apiClient, scriptExecutor, scriptGeneratorService, logger);
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
                        var interactiveWorkflow = new InteractiveModeWorkflow(
                            apiClient,
                            packageSelector,
                            scriptExecutor,
                            scriptGeneratorService,
                            versionCheckService,
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

            // Display completion message
            AnsiConsole.MarkupLine("\n[green]✓ Process completed successfully![/]");
            logger.LogInformation("PSW CLI completed successfully");
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
}
