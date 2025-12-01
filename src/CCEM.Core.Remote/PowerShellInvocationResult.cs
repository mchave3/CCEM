namespace CCEM.Core.Remote;

/// <summary>
/// Result of a PowerShell invocation.
/// </summary>
public sealed record PowerShellInvocationResult(
    IReadOnlyList<string> Output,
    IReadOnlyList<string> Errors,
    bool HadErrors,
    bool TimedOut);
