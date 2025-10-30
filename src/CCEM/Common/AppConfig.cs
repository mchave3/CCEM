using Nucs.JsonSettings.Examples;
using Nucs.JsonSettings.Modulation;

namespace CCEM.Common;

[GenerateAutoSaveOnChange]
public partial class AppConfig : NotifiyingJsonSettings, IVersionable
{
    [EnforcedVersion("1.0.0.0")]
    public Version Version { get; set; } = new Version(1, 0, 0, 0);

    private string fileName { get; set; } = Constants.AppConfigPath;

    private bool useDeveloperMode { get; set; }
    private string lastUpdateCheck { get; set; }
    private string updateChannel { get; set; } = "Stable";
    private string lastInstalledChannel { get; set; } = "Stable";
    private bool isUpdateChannelOverridden { get; set; }

    // Docs: https://github.com/Nucs/JsonSettings
}
