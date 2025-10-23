using Serilog;

namespace CCEM.Core.Logger;

/// <summary>
/// Options used to configure Serilog for the application.
/// </summary>
public sealed class LoggerConfigurationOptions
{
    /// <summary>
    /// Optional version value added as an enricher.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// The log directory created when <see cref="CreateLogDirectory"/> is true.
    /// </summary>
    public string? LogDirectoryPath { get; init; }

    /// <summary>
    /// The fully qualified log file path for the file sink.
    /// </summary>
    public string? LogFilePath { get; init; }

    /// <summary>
    /// Determines how frequently the file sink rolls the log file.
    /// </summary>
    public RollingInterval RollingInterval { get; init; } = RollingInterval.Day;

    /// <summary>
    /// Controls whether the file sink is added.
    /// </summary>
    public bool EnableFileSink { get; init; } = true;

    /// <summary>
    /// Controls whether the debug sink is added.
    /// </summary>
    public bool EnableDebugSink { get; init; } = true;

    /// <summary>
    /// Creates the log directory when the directory path is provided.
    /// </summary>
    public bool CreateLogDirectory { get; init; } = true;

    /// <summary>
    /// Limits the number of retained log files for the file sink.
    /// </summary>
    public int? RetainedFileCountLimit { get; init; }

    /// <summary>
    /// Rolls the log file when the size limit is reached.
    /// </summary>
    public bool RollOnFileSizeLimit { get; init; }

    /// <summary>
    /// Shares the log file for multi-process scenarios.
    /// </summary>
    public bool Shared { get; init; }
}
