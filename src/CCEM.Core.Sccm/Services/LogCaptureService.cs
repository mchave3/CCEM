using CCEM.Core.Sccm.Contracts;
using CCEM.Core.Sccm.Models;

namespace CCEM.Core.Sccm.Services;

/// <summary>
/// Placeholder log capture service; wired for async streaming.
/// </summary>
public sealed class LogCaptureService : ILogCaptureService
{
    public async IAsyncEnumerable<string> TailAsync(
        SccmConnectionInfo connection,
        string logPath,
        int maxLines = 200,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        if (string.IsNullOrWhiteSpace(logPath))
        {
            throw new ArgumentException("Log path is required.", nameof(logPath));
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Placeholder: actual implementation will stream remote logs via PowerShell (Get-Content -Tail).
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }
}
