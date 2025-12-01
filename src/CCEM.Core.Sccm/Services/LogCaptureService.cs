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

        // Placeholder: emits sample lines; replace with remote Get-Content -Tail implementation.
        var sample = new[]
        {
            $"[{DateTimeOffset.Now:HH:mm:ss}] CCMExec: Heartbeat OK",
            $"[{DateTimeOffset.Now:HH:mm:ss}] Updates: Scan started",
            $"[{DateTimeOffset.Now:HH:mm:ss}] Updates: Scan completed"
        };

        foreach (var line in sample.Take(maxLines))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return line;
            await Task.Delay(10, cancellationToken).ConfigureAwait(false);
        }
    }
}
