using CCEM.Core.Velopack.Models;
using Velopack;

namespace CCEM.Core.Velopack.Services;

/// <summary>
/// Provides an abstraction for checking, downloading, and applying Velopack updates.
/// </summary>
public interface IVelopackUpdateService
{
    /// <summary>
    /// Checks the configured update source for a newer release.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the check operation.</param>
    /// <returns>Structured information about the update result.</returns>
    Task<VelopackUpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the assets required to apply the specified update.
    /// </summary>
    /// <param name="update">The update to download.</param>
    /// <param name="progress">Optional callback invoked with download progress (0-100).</param>
    /// <param name="cancellationToken">Token used to cancel the download operation.</param>
    Task DownloadUpdatesAsync(VelopackUpdateCheckResult update, Action<int>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies the downloaded update and restarts the host application.
    /// </summary>
    /// <param name="update">The update payload to apply.</param>
    /// <param name="restartArgs">Optional command-line arguments for the restarted process.</param>
    void ApplyUpdatesAndRestart(VelopackUpdateCheckResult update, string[]? restartArgs = null);

    /// <summary>
    /// Waits for the host process to exit before applying the update, optionally starting the application afterwards.
    /// </summary>
    /// <param name="update">The update payload to apply.</param>
    /// <param name="silent">Whether the update process runs without UI.</param>
    /// <param name="restart">True to restart the application after applying the update; otherwise false.</param>
    /// <param name="restartArgs">Optional command-line arguments for the restarted process.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task WaitExitThenApplyUpdatesAsync(VelopackUpdateCheckResult update, bool silent = false, bool restart = true, string[]? restartArgs = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the application is running from an installed Velopack bundle.
    /// </summary>
    /// <returns>True when running from an installed bundle; otherwise false.</returns>
    bool IsRunningInstalledVersion();

    /// <summary>
    /// Gets the version currently installed on the local machine, if available.
    /// </summary>
    /// <returns>The installed application version, or null when unknown.</returns>
    string? GetCurrentlyInstalledVersion();

    /// <summary>
    /// Gets the Velopack release channel currently targeted for updates.
    /// </summary>
    VelopackChannel CurrentChannel { get; }

    /// <summary>
    /// Sets the release channel that should be used when checking for updates.
    /// </summary>
    /// <param name="channel">The desired Velopack release channel.</param>
    void SetChannel(VelopackChannel channel);
}
