using CCEM.Core.Logger;
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
    private readonly Func<VelopackUpdateConfiguration, VelopackChannel, UpdateManager> _managerFactory;
    private readonly IAppLogger? _logger;
    private VelopackChannel _channel = VelopackChannel.Stable;

    /// <summary>
    /// Creates a new update service using the supplied Velopack configuration.
    /// </summary>
    /// <param name="configuration">Provides repository and authentication settings.</param>
    /// <param name="managerFactory">Optional factory used to create <see cref="UpdateManager"/> instances.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    public VelopackUpdateService(
        VelopackUpdateConfiguration configuration,
        Func<VelopackUpdateConfiguration, VelopackChannel, UpdateManager>? managerFactory = null,
        IAppLogger? logger = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _managerFactory = managerFactory ?? CreateDefaultManager;
        _logger = logger;

        Logger?.Debug("Initialized VelopackUpdateService for repository {RepositoryUrl} with default channel {Channel}", _configuration.RepositoryUrl, _channel);
    }

    /// <inheritdoc />
    public async Task<VelopackUpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        Logger?.Information("Checking for updates from repository {RepositoryUrl} on channel {Channel}", _configuration.RepositoryUrl, _channel);
        try
        {
            var manager = CreateManager();

            cancellationToken.ThrowIfCancellationRequested();

            var updateInfo = await manager.CheckForUpdatesAsync().ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var result = new VelopackUpdateCheckResult(
                updateInfo is not null,
                manager.CurrentVersion?.ToString(),
                updateInfo?.TargetFullRelease?.Version?.ToString(),
                updateInfo?.TargetFullRelease?.NotesMarkdown,
                updateInfo?.TargetFullRelease?.NotesHTML,
                updateInfo?.IsDowngrade ?? false,
                updateInfo);

            Logger?.Information(
                "Update check completed. UpdateAvailable={UpdateAvailable}, CurrentVersion={CurrentVersion}, AvailableVersion={AvailableVersion}, IsDowngrade={IsDowngrade}",
                result.IsUpdateAvailable,
                result.CurrentVersion ?? "unknown",
                result.AvailableVersion ?? "unknown",
                result.IsDowngrade);

            return result;
        }
        catch (OperationCanceledException)
        {
            Logger?.Information("Update check canceled by caller.");
            throw;
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Failed to check for updates on channel {Channel}", _channel);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DownloadUpdatesAsync(VelopackUpdateCheckResult update, Action<int>? progress = null, CancellationToken cancellationToken = default)
    {
        if (update is null)
        {
            Logger?.Error("DownloadUpdatesAsync invoked with a null update result.");
            throw new ArgumentNullException(nameof(update));
        }

        if (update.UpdateInfo is null)
        {
            Logger?.Error("DownloadUpdatesAsync invoked without update metadata.");
            throw new InvalidOperationException("There is no update information available to download.");
        }

        var targetVersion = ResolveVersion(update);
        Logger?.Information("Starting download for update version {Version} on channel {Channel}", targetVersion, _channel);

        try
        {
            var manager = CreateManager();

            cancellationToken.ThrowIfCancellationRequested();

            await manager.DownloadUpdatesAsync(update.UpdateInfo, progress, cancellationToken).ConfigureAwait(false);

            Logger?.Information("Completed download for update version {Version}", targetVersion);
        }
        catch (OperationCanceledException)
        {
            Logger?.Information("Download canceled for update version {Version}", targetVersion);
            throw;
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Failed to download update version {Version}", targetVersion);
            throw;
        }
    }

    /// <inheritdoc />
    public void ApplyUpdatesAndRestart(VelopackUpdateCheckResult update, string[]? restartArgs = null)
    {
        if (update is null)
        {
            Logger?.Error("ApplyUpdatesAndRestart invoked with a null update result.");
            throw new ArgumentNullException(nameof(update));
        }

        if (update.TargetRelease is null)
        {
            Logger?.Error("ApplyUpdatesAndRestart invoked without a target release.");
            throw new InvalidOperationException("The update information does not contain a target release.");
        }

        var targetVersion = ResolveVersion(update);
        Logger?.Information("Applying update version {Version} and restarting application.", targetVersion);

        try
        {
            var manager = CreateManager();
            manager.ApplyUpdatesAndRestart(update.TargetRelease, restartArgs);
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Failed to apply update version {Version}", targetVersion);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task WaitExitThenApplyUpdatesAsync(VelopackUpdateCheckResult update, bool silent = false, bool restart = true, string[]? restartArgs = null, CancellationToken cancellationToken = default)
    {
        if (update is null)
        {
            Logger?.Error("WaitExitThenApplyUpdatesAsync invoked with a null update result.");
            throw new ArgumentNullException(nameof(update));
        }

        if (update.TargetRelease is null)
        {
            Logger?.Error("WaitExitThenApplyUpdatesAsync invoked without a target release.");
            throw new InvalidOperationException("The update information does not contain a target release.");
        }

        var targetVersion = ResolveVersion(update);
        Logger?.Information("Waiting for exit before applying update version {Version}. Silent={Silent}, Restart={Restart}", targetVersion, silent, restart);

        try
        {
            var manager = CreateManager();

            cancellationToken.ThrowIfCancellationRequested();

            await manager.WaitExitThenApplyUpdatesAsync(update.TargetRelease, silent, restart, restartArgs).ConfigureAwait(false);

            Logger?.Information("Successfully applied update version {Version} after process exit.", targetVersion);
        }
        catch (OperationCanceledException)
        {
            Logger?.Information("Deferred apply canceled for update version {Version}", targetVersion);
            throw;
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Failed during deferred apply for update version {Version}", targetVersion);
            throw;
        }
    }

    /// <inheritdoc />
    public bool IsRunningInstalledVersion()
    {
        try
        {
            var manager = CreateManager();
            var isInstalled = manager.IsInstalled;
            Logger?.Debug("IsRunningInstalledVersion returned {IsInstalled}", isInstalled);
            return isInstalled;
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Failed to determine if running installed version.");
            throw;
        }
    }

    /// <inheritdoc />
    public string? GetCurrentlyInstalledVersion()
    {
        try
        {
            var manager = CreateManager();
            var currentVersion = manager.CurrentVersion?.ToString();
            Logger?.Debug("Detected currently installed version {Version}", currentVersion ?? "unknown");
            return currentVersion;
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Failed to retrieve the currently installed version.");
            throw;
        }
    }

    /// <inheritdoc />
    public VelopackChannel CurrentChannel => _channel;

    /// <inheritdoc />
    public void SetChannel(VelopackChannel channel)
    {
        if (_channel == channel)
        {
            Logger?.Debug("SetChannel invoked with existing channel {Channel}", channel);
            return;
        }

        var previousChannel = _channel;
        _channel = channel;
        Logger?.Information("Update channel changed from {PreviousChannel} to {Channel}", previousChannel, _channel);
    }

    private UpdateManager CreateManager()
    {
        try
        {
            Logger?.Debug("Creating UpdateManager for channel {Channel}", _channel);
            return _managerFactory(_configuration, _channel);
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Failed to create UpdateManager for channel {Channel}", _channel);
            throw;
        }
    }

    private static UpdateManager CreateDefaultManager(
        VelopackUpdateConfiguration configuration,
        VelopackChannel channel)
    {
        var includePrerelease = channel == VelopackChannel.Nightly;

        var source = new GithubSource(
            configuration.RepositoryUrl,
            configuration.AccessToken ?? string.Empty,
            includePrerelease,
            downloader: null);

        var options = new UpdateOptions
        {
            ExplicitChannel = channel switch
            {
                VelopackChannel.Stable => "stable",
                VelopackChannel.Nightly => "nightly",
                _ => null
            },
            AllowVersionDowngrade = true
        };

        return new UpdateManager(source, options, locator: null);
    }

    private IAppLogger? Logger => _logger ?? LoggerSetup.Logger;

    private static string ResolveVersion(VelopackUpdateCheckResult update) =>
        update.TargetRelease?.Version?.ToString()
        ?? update.AvailableVersion
        ?? update.UpdateInfo?.TargetFullRelease?.Version?.ToString()
        ?? "unknown";
}
