using System.IO;
using Serilog;

namespace CCEM.Core.Logger;

/// <summary>
/// Provides a single entry point to configure Serilog for the application while keeping
/// Serilog details internal to this assembly.
/// </summary>
public static class LoggerSetup
{
    private static readonly object SyncRoot = new();
    private static ILogger? _serilogLogger;
    private static IAppLogger? _adapter;

    /// <summary>
    /// Gets the configured logger adapter, or null when logging has not been configured.
    /// </summary>
    public static IAppLogger? Logger => _adapter;

    /// <summary>
    /// Configures the underlying Serilog logger using the supplied options.
    /// </summary>
    /// <param name="options">User supplied logging options.</param>
    public static void ConfigureLogger(LoggerConfigurationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var loggerConfiguration = new LoggerConfiguration();

        if (!string.IsNullOrWhiteSpace(options.Version))
        {
            loggerConfiguration = loggerConfiguration.Enrich.WithProperty("Version", options.Version);
        }

        if (options.EnableFileSink && !string.IsNullOrWhiteSpace(options.LogFilePath))
        {
            var directoryPath = options.LogDirectoryPath ?? Path.GetDirectoryName(options.LogFilePath);
            if (!string.IsNullOrWhiteSpace(directoryPath) && options.CreateLogDirectory)
            {
                Directory.CreateDirectory(directoryPath);
            }

            loggerConfiguration = loggerConfiguration.WriteTo.File(
                path: options.LogFilePath,
                rollingInterval: MapRollingInterval(options.RollingInterval),
                retainedFileCountLimit: options.RetainedFileCountLimit,
                rollOnFileSizeLimit: options.RollOnFileSizeLimit,
                shared: options.Shared
            );
        }

        if (options.EnableDebugSink)
        {
            loggerConfiguration = loggerConfiguration.WriteTo.Debug();
        }

        lock (SyncRoot)
        {
            (_serilogLogger as IDisposable)?.Dispose();

            _serilogLogger = loggerConfiguration.CreateLogger();
            _adapter = new SerilogAppLogger(_serilogLogger);
        }
    }

    /// <summary>
    /// Disposes the existing logger instance if one has been created.
    /// </summary>
    public static void CloseAndFlush()
    {
        lock (SyncRoot)
        {
            (_serilogLogger as IDisposable)?.Dispose();
            _serilogLogger = null;
            _adapter = null;
        }
    }

    private static RollingInterval MapRollingInterval(LogRollingInterval interval) =>
        interval switch
        {
            LogRollingInterval.Infinite => RollingInterval.Infinite,
            LogRollingInterval.Year => RollingInterval.Year,
            LogRollingInterval.Month => RollingInterval.Month,
            LogRollingInterval.Day => RollingInterval.Day,
            LogRollingInterval.Hour => RollingInterval.Hour,
            LogRollingInterval.Minute => RollingInterval.Minute,
            _ => RollingInterval.Day
        };
}
