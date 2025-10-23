using CCEM.Core.Velopack.Models;
using Velopack;
using Velopack.Sources;

namespace CCEM.Core.Velopack.Services;

/// <summary>
/// Provides Velopack-based update management including discovery, download, and installation workflows.
/// </summary>
public sealed class VelopackUpdateService : IVelopackUpdateService
{
    private readonly VelopackUpdateConfiguration _configuration;
    private VelopackChannel _channel = VelopackChannel.Stable;

    /// <summary>
    /// Creates a new update service using the supplied Velopack configuration.
    /// </summary>
    /// <param name="configuration">Provides repository and authentication settings.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public VelopackUpdateService(VelopackUpdateConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public bool IsRunningInstalledVersion()
    {
        var manager = CreateManager();
        return manager.IsInstalled;
    }

    /// <inheritdoc />
    public string? GetCurrentlyInstalledVersion()
    {
        var manager = CreateManager();
        return manager.CurrentVersion?.ToString();
    }

    /// <inheritdoc />
    public VelopackChannel CurrentChannel => _channel;

    /// <inheritdoc />
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
            AllowVersionDowngrade = true
        };

        return new UpdateManager(source, options, locator: null);
    }
}
