using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace PackageCliTool.Logging;

/// <summary>
/// Sets up and configures logging for the application
/// </summary>
public static class LoggerSetup
{
    private static ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Initializes the logging infrastructure
    /// </summary>
    /// <param name="verboseMode">Enable verbose logging</param>
    /// <param name="enableFileLogging">Enable logging to file</param>
    public static void Initialize(bool verboseMode = false, bool enableFileLogging = true)
    {
        var logLevel = verboseMode ? LogEventLevel.Debug : LogEventLevel.Information;
        var logDirectory = GetLogDirectory();

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "PSW-CLI");

        if (enableFileLogging)
        {
            EnsureLogDirectoryExists(logDirectory);

            var logFilePath = Path.Combine(logDirectory, $"psw-{DateTime.Now:yyyyMMdd}.log");

            loggerConfig.WriteTo.File(
                logFilePath,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true
            );
        }

        Log.Logger = loggerConfig.CreateLogger();

        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(Log.Logger);
        });
    }

    /// <summary>
    /// Creates a logger for the specified type
    /// </summary>
    public static ILogger<T> CreateLogger<T>()
    {
        if (_loggerFactory == null)
        {
            Initialize();
        }

        return _loggerFactory!.CreateLogger<T>();
    }

    /// <summary>
    /// Creates a logger with the specified category name
    /// </summary>
    public static ILogger CreateLogger(string categoryName)
    {
        if (_loggerFactory == null)
        {
            Initialize();
        }

        return _loggerFactory!.CreateLogger(categoryName);
    }

    /// <summary>
    /// Gets the log directory path
    /// </summary>
    private static string GetLogDirectory()
    {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(homeDirectory, ".psw", "logs");
    }

    /// <summary>
    /// Ensures the log directory exists
    /// </summary>
    private static void EnsureLogDirectoryExists(string logDirectory)
    {
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
    }

    /// <summary>
    /// Flushes and closes the logger
    /// </summary>
    public static void Shutdown()
    {
        Log.CloseAndFlush();
        _loggerFactory?.Dispose();
    }
}
