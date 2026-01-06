using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Automation.functions;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record SccmComponentRow(
    string Name,
    string DisplayName,
    string? Version,
    bool? Enabled,
    string EnabledText);

public sealed partial class SccmComponentsViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmComponentsViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmComponentRow> Components { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected)
        {
            StatusMessage = "Not connected.";
            Components.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Components.Clear();

        try
        {
            var rows = await Task.Run(() =>
            {
                var list = agent.Client.Components.InstalledComponents;
                return list.Select(c =>
                {
                    var enabledText = c.Enabled switch
                    {
                        true => "Enabled",
                        false => "Disabled",
                        _ => "Unknown",
                    };

                    return new SccmComponentRow(
                        Name: c.Name ?? string.Empty,
                        DisplayName: string.IsNullOrWhiteSpace(c.DisplayName) ? (c.Name ?? string.Empty) : c.DisplayName,
                        Version: c.Version,
                        Enabled: c.Enabled,
                        EnabledText: enabledText);
                }).ToList();
            }).ConfigureAwait(false);

            foreach (var row in rows.OrderBy(r => r.DisplayName))
            {
                Components.Add(row);
            }

            StatusMessage = $"Loaded {Components.Count} components.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load components: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

