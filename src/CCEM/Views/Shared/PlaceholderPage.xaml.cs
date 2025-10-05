using System.Collections.Generic;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace CCEM.Views.Shared;

public sealed partial class PlaceholderPage : Page
{
    private static readonly IReadOnlyDictionary<string, PlaceholderMetadata> Metadata = new Dictionary<string, PlaceholderMetadata>(StringComparer.OrdinalIgnoreCase)
    {
        ["SCCM_OVERVIEW"] = new("Client overview", "A consolidated client summary page will arrive in Phase 3.", "\uE8BC"),
        ["SCCM_COMPONENTS"] = new("Components", "Detailed component health data will be migrated with the Components grid.", "\uE9F9"),
        ["SCCM_AGENT_SETTINGS"] = new("Agent settings", "Agent configuration management will be available after the control migration.", "\uE713"),
        ["SCCM_INSTALL_REPAIR"] = new("Install or repair", "WinUI actions for installing and repairing the client are on the roadmap.", "\uE90F"),
        ["SCCM_INSTALL_AGENT"] = new("Install agent", "The agent deployment wizard is being rebuilt for WinUI.", "\uE896"),
        ["SCCM_CACHE"] = new("Cache", "Cache inspection tools will return in the inventory sprint.", "\uE895"),
        ["SCCM_APPLICATIONS"] = new("Applications", "Application insights are moving to WinUI data grids in Phase 3.", "\uE8B7"),
        ["SCCM_INSTALLED_SOFTWARE"] = new("Installed software", "Software inventory will be restored with export options soon.", "\uE74C"),
        ["SCCM_ADVERTISEMENTS"] = new("Advertisements", "Deployment history tooling is scheduled for the control migration.", "\uE789"),
        ["SCCM_EXECUTION_HISTORY"] = new("Execution history", "Task execution timelines will reappear shortly.", "\uE81C"),
        ["SCCM_UPDATES"] = new("Software updates", "Compliance dashboards are moving to WinUI.", "\uE777"),
        ["SCCM_ALL_UPDATES"] = new("All updates", "Historical update data is being migrated.", "\uE8FD"),
        ["SCCM_UPDATE_STATUS"] = new("Update status", "Client update status will be available after migration.", "\uE7BA"),
        ["SCCM_SERVICES"] = new("Services", "Service management tooling is being modernised.", "\uE950"),
        ["SCCM_PROCESSES"] = new("Processes", "Real-time process insights are planned for Phase 3.", "\uE9D9"),
        ["SCCM_LOGS"] = new("Logs", "The log explorer experience is in development.", "\uE8A5"),
        ["SCCM_LOG_VIEWER"] = new("Log viewer", "Live log streaming returns with the diagnostics work.", "\uE8A5"),
        ["SCCM_EVENT_MONITORING"] = new("Event monitoring", "Event viewer integration is queued for Phase 3.", "\uE7C1"),
        ["SCCM_SERVICE_WINDOWS"] = new("Service windows", "Service window management is being redesigned for WinUI.", "\uE787"),
        ["SCCM_VARIABLES"] = new("Variables", "Collection variable editing will return with advanced tooling.", "\uE8CB"),
        ["SCCM_POWER"] = new("Power management", "Power configuration workflows are under construction.", "\uE945"),
        ["SCCM_WMI_BROWSER"] = new("WMI browser", "A modern WMI browser is being prototyped.", "\uE8B7"),
        ["SCCM_EVALUATION"] = new("Evaluation", "Evaluation cycle controls are part of the upcoming migration.", "\uE9D5"),
        ["SCCM_IMPORT_APP"] = new("Import application", "Application import workflows are being modernised.", "\uE8B5"),
        ["INTUNE_COMING_SOON"] = new("Intune workspace", "Microsoft Intune integration will follow after SCCM parity.", "\uE753"),
    };

    public PlaceholderPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is PlaceholderNavigationContext context)
        {
            ApplyMetadata(context);
        }
        else if (e.Parameter is string token && Metadata.TryGetValue(token, out var metadata))
        {
            ApplyMetadata(metadata);
        }
        else
        {
            ApplyMetadata(new PlaceholderMetadata("Coming soon", "This area is being prepared for migration.", "\uE823"));
        }
    }

    private void ApplyMetadata(PlaceholderMetadata metadata)
    {
        TitleText.Text = metadata.Title;
        DescriptionText.Text = metadata.Description;
        GlyphIcon.Glyph = metadata.Glyph;
        GlyphIcon.Foreground = metadata.Glyph switch
        {
            "\uE753" => new SolidColorBrush(Colors.Teal),
            "\uE8A5" => new SolidColorBrush(Colors.Orange),
            "\uE945" => new SolidColorBrush(Colors.Green),
            _ => GlyphIcon.Foreground
        };
    }

    private readonly record struct PlaceholderMetadata(string Title, string Description, string Glyph);

    public sealed class PlaceholderNavigationContext
    {
        public PlaceholderNavigationContext(string title, string description, string glyph = "\uE823")
        {
            Title = title;
            Description = description;
            Glyph = glyph;
        }

        public string Title { get; }
        public string Description { get; }
        public string Glyph { get; }
    }
}
