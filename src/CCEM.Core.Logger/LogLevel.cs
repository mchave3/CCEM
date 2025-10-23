namespace CCEM.Core.Logger;

/// <summary>
/// Logical logging levels exposed to application code without leaking Serilog types.
/// </summary>
public enum LogLevel
{
    Verbose,
    Debug,
    Information,
    Warning,
    Error,
    Fatal
}
