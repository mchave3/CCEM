using Velopack;

namespace CCEM.Core.Velopack.Models;

public sealed class VelopackUpdateCheckResult
{
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

    public bool IsUpdateAvailable { get; }
    public string? CurrentVersion { get; }
    public string? AvailableVersion { get; }
    public string? ReleaseNotesMarkdown { get; }
    public string? ReleaseNotesHtml { get; }
    public bool IsDowngrade { get; }
    public UpdateInfo? UpdateInfo { get; }
    public VelopackAsset? TargetRelease => UpdateInfo?.TargetFullRelease;
}
