using CCEM.Core.Velopack.Models;
using Velopack;
using Velopack.Sources;

namespace CCEM.Core.Velopack.Services;

public sealed class VelopackUpdateService : IVelopackUpdateService
{
    private readonly VelopackUpdateConfiguration _configuration;
    private VelopackChannel _channel = VelopackChannel.Stable;

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

    public async Task DownloadUpdatesAsync(VelopackUpdateCheckResult update, Action<int>? progress = null, CancellationToken cancellationToken = default)
    {
        if (update is null)
        {
            throw new ArgumentNullException(nameof(update));
        }

        if (update.UpdateInfo is null)
        {
            throw new InvalidOperationException("There is no update information available to download.");
        }

        var manager = CreateManager();

        cancellationToken.ThrowIfCancellationRequested();

        await manager.DownloadUpdatesAsync(update.UpdateInfo, progress, cancellationToken).ConfigureAwait(false);
    }

    public void ApplyUpdatesAndRestart(VelopackUpdateCheckResult update, string[]? restartArgs = null)
    {
        if (update is null)
        {
            throw new ArgumentNullException(nameof(update));
        }

        if (update.TargetRelease is null)
        {
            throw new InvalidOperationException("The update information does not contain a target release.");
        }

        var manager = CreateManager();
        manager.ApplyUpdatesAndRestart(update.TargetRelease, restartArgs);
    }

    public async Task WaitExitThenApplyUpdatesAsync(VelopackUpdateCheckResult update, bool silent = false, bool restart = true, string[]? restartArgs = null, CancellationToken cancellationToken = default)
    {
        if (update is null)
        {
            throw new ArgumentNullException(nameof(update));
        }

        if (update.TargetRelease is null)
        {
            throw new InvalidOperationException("The update information does not contain a target release.");
        }

        var manager = CreateManager();

        cancellationToken.ThrowIfCancellationRequested();

        await manager.WaitExitThenApplyUpdatesAsync(update.TargetRelease, silent, restart, restartArgs).ConfigureAwait(false);
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

    public VelopackChannel CurrentChannel => _channel;

    public void SetChannel(VelopackChannel channel)
    {
        _channel = channel;
    }

    private UpdateManager CreateManager()
    {
        var includePrerelease = _channel == VelopackChannel.Nightly;

        var source = new GithubSource(
            _configuration.RepositoryUrl,
            _configuration.AccessToken ?? string.Empty,
            includePrerelease,
            downloader: null);

        var options = new UpdateOptions
        {
            ExplicitChannel = _channel switch
            {
                VelopackChannel.Stable => "stable",
                VelopackChannel.Nightly => "nightly",
                _ => null
            },
            AllowVersionDowngrade = _channel == VelopackChannel.Stable
        };

        return new UpdateManager(source, options, locator: null);
    }
}
