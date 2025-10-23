namespace CCEM.Core.Logger;

/// <summary>
/// Determines how frequently the log file rolls over.
/// </summary>
public enum LogRollingInterval
{
    Infinite,
    Year,
    Month,
    Day,
    Hour,
    Minute
}
