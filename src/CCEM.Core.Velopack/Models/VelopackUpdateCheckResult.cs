using Velopack;

namespace CCEM.Core.Velopack.Models;

/// <summary>
/// Represents the outcome of a Velopack update check, including release metadata and payload context.
/// </summary>
public sealed class VelopackUpdateCheckResult
{
    /// <summary>
    /// Initializes a new update check result using information reported by Velopack.
    /// </summary>
    /// <param name="isUpdateAvailable">True when a newer release has been discovered.</param>
    /// <param name="currentVersion">The version currently installed on the local machine.</param>
    /// <param name="availableVersion">The version advertised by the update feed.</param>
    /// <param name="releaseNotesMarkdown">Release notes rendered as Markdown, when supplied.</param>
    /// <param name="releaseNotesHtml">Release notes rendered as HTML, when supplied.</param>
    /// <param name="isDowngrade">True when the target release is older than the installed version.</param>
    /// <param name="updateInfo">The raw Velopack update metadata payload.</param>
    public VelopackUpdateCheckResult(
        bool isUpdateAvailable,
        string? currentVersion,
        string? availableVersion,
        string? releaseNotesMarkdown,
        string? releaseNotesHtml,
        bool isDowngrade,
        UpdateInfo? updateInfo)
    {
        IsUpdateAvailable = isUpdateAvailable;
        CurrentVersion = currentVersion;
        AvailableVersion = availableVersion;
        ReleaseNotesMarkdown = releaseNotesMarkdown;
        ReleaseNotesHtml = releaseNotesHtml;
        IsDowngrade = isDowngrade;
        UpdateInfo = updateInfo;
    }

    /// <summary>
    /// Gets a value indicating whether a newer release is available.
    /// </summary>
    public bool IsUpdateAvailable { get; }

    /// <summary>
    /// Gets the currently installed version, if known.
    /// </summary>
    public string? CurrentVersion { get; }

    /// <summary>
    /// Gets the version supplied by the update feed, if any.
    /// </summary>
    public string? AvailableVersion { get; }

    /// <summary>
    /// Gets the Markdown representation of the release notes, when provided.
    /// </summary>
    public string? ReleaseNotesMarkdown { get; }

    /// <summary>
    /// Gets the HTML representation of the release notes, when provided.
    /// </summary>
    public string? ReleaseNotesHtml { get; }

    /// <summary>
    /// Gets a value indicating whether the update would downgrade the application.
    /// </summary>
    public bool IsDowngrade { get; }

    /// <summary>
    /// Gets the raw Velopack update metadata used for downloading assets.
    /// </summary>
    internal UpdateInfo? UpdateInfo { get; }

    /// <summary>
    /// Gets the full-release asset resolved for this update, when available.
    /// </summary>
    internal VelopackAsset? TargetRelease => UpdateInfo?.TargetFullRelease;
}
