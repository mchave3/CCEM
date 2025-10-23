using CCEM.Core.Velopack.Models;
using Velopack;

namespace CCEM.Core.Velopack.Services;

public interface IVelopackUpdateService
{
    Task<VelopackUpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default);
    Task DownloadUpdatesAsync(UpdateInfo updateInfo, Action<int>? progress = null, CancellationToken cancellationToken = default);
    void ApplyUpdatesAndRestart(UpdateInfo updateInfo, string[]? restartArgs = null);
    Task WaitExitThenApplyUpdatesAsync(UpdateInfo updateInfo, bool silent = false, bool restart = true, string[]? restartArgs = null, CancellationToken cancellationToken = default);
    bool IsRunningInstalledVersion();
    string? GetCurrentlyInstalledVersion();
}
