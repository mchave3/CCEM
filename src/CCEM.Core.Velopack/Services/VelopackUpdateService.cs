using CCEM.Core.Velopack.Models;
using Velopack;
using Velopack.Sources;

namespace CCEM.Core.Velopack.Services;

public sealed class VelopackUpdateService : IVelopackUpdateService
{
    private readonly VelopackUpdateConfiguration _configuration;

    public VelopackUpdateService(VelopackUpdateConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<VelopackUpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        var manager = CreateManager();

        cancellationToken.ThrowIfCancellationRequested();

        var updateInfo = await manager.CheckForUpdatesAsync().ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new VelopackUpdateCheckResult(
            updateInfo is not null,
            manager.CurrentVersion?.ToString(),
            updateInfo?.TargetFullRelease?.Version?.ToString(),
            updateInfo?.TargetFullRelease?.NotesMarkdown,
            updateInfo?.TargetFullRelease?.NotesHTML,
            updateInfo?.IsDowngrade ?? false,
            updateInfo);
    }

    public async Task DownloadUpdatesAsync(UpdateInfo updateInfo, Action<int>? progress = null, CancellationToken cancellationToken = default)
    {
        if (updateInfo is null)
        {
            throw new ArgumentNullException(nameof(updateInfo));
        }

        var manager = CreateManager();

        cancellationToken.ThrowIfCancellationRequested();

        await manager.DownloadUpdatesAsync(updateInfo, progress, cancellationToken).ConfigureAwait(false);
    }

    public void ApplyUpdatesAndRestart(UpdateInfo updateInfo, string[]? restartArgs = null)
    {
        if (updateInfo is null)
        {
            throw new ArgumentNullException(nameof(updateInfo));
        }

        var targetRelease = updateInfo.TargetFullRelease ?? throw new InvalidOperationException("The update information does not contain a target release.");

        var manager = CreateManager();
        manager.ApplyUpdatesAndRestart(targetRelease, restartArgs);
    }

    public async Task WaitExitThenApplyUpdatesAsync(UpdateInfo updateInfo, bool silent = false, bool restart = true, string[]? restartArgs = null, CancellationToken cancellationToken = default)
    {
        if (updateInfo is null)
        {
            throw new ArgumentNullException(nameof(updateInfo));
        }

        var targetRelease = updateInfo.TargetFullRelease ?? throw new InvalidOperationException("The update information does not contain a target release.");

        var manager = CreateManager();

        cancellationToken.ThrowIfCancellationRequested();

        await manager.WaitExitThenApplyUpdatesAsync(targetRelease, silent, restart, restartArgs).ConfigureAwait(false);
    }

    public bool IsRunningInstalledVersion()
    {
        var manager = CreateManager();
        return manager.IsInstalled;
    }

    public string? GetCurrentlyInstalledVersion()
    {
        var manager = CreateManager();
        return manager.CurrentVersion?.ToString();
    }

    private UpdateManager CreateManager()
    {
        var source = new GithubSource(
            _configuration.RepositoryUrl,
            _configuration.AccessToken ?? string.Empty,
            _configuration.IncludePrerelease,
            downloader: null);

        UpdateOptions? options = null;
        if (!string.IsNullOrWhiteSpace(_configuration.ExplicitChannel) || _configuration.AllowVersionDowngrade)
        {
            options = new UpdateOptions
            {
                ExplicitChannel = _configuration.ExplicitChannel,
                AllowVersionDowngrade = _configuration.AllowVersionDowngrade
            };
        }

        return new UpdateManager(source, options, locator: null);
    }
}
