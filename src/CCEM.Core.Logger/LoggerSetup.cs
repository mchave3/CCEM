using System.IO;
using Serilog;

namespace CCEM.Core.Logger;

/// <summary>
/// Provides a single entry point to configure Serilog for the application.
/// </summary>
public static class LoggerSetup
{
    private static readonly object SyncRoot = new();

    /// <summary>
    /// Gets the configured <see cref="ILogger"/> instance when available.
    /// </summary>
    public static ILogger? Logger { get; private set; }

    /// <summary>
    /// Configures Serilog using the supplied options and optional configuration hook.
    /// </summary>
    /// <param name="options">User supplied logging options.</param>
    /// <param name="configure">Optional hook for additional sink/enricher configuration.</param>
    public static void ConfigureLogger(LoggerConfigurationOptions options, Action<LoggerConfiguration>? configure = null)
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
                rollingInterval: options.RollingInterval,
                retainedFileCountLimit: options.RetainedFileCountLimit,
                rollOnFileSizeLimit: options.RollOnFileSizeLimit,
                shared: options.Shared
            );
        }

        if (options.EnableDebugSink)
        {
            loggerConfiguration = loggerConfiguration.WriteTo.Debug();
        }

        configure?.Invoke(loggerConfiguration);

        lock (SyncRoot)
        {
            (Logger as IDisposable)?.Dispose();
            Logger = loggerConfiguration.CreateLogger();
        }
    }

    /// <summary>
    /// Disposes the existing logger instance if one has been created.
    /// </summary>
    public static void CloseAndFlush()
    {
        lock (SyncRoot)
        {
            (Logger as IDisposable)?.Dispose();
            Logger = null;
        }
    }
}
