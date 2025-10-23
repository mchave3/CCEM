using CCEM.Core.Velopack.Models;
using Velopack;

namespace CCEM.Core.Velopack.Services;

public interface IVelopackUpdateService
{
    Task<VelopackUpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default);
    Task DownloadUpdatesAsync(VelopackUpdateCheckResult update, Action<int>? progress = null, CancellationToken cancellationToken = default);
    void ApplyUpdatesAndRestart(VelopackUpdateCheckResult update, string[]? restartArgs = null);
    Task WaitExitThenApplyUpdatesAsync(VelopackUpdateCheckResult update, bool silent = false, bool restart = true, string[]? restartArgs = null, CancellationToken cancellationToken = default);
    bool IsRunningInstalledVersion();
    string? GetCurrentlyInstalledVersion();
}
