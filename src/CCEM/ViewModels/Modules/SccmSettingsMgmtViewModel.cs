using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Automation.policy;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record SccmComponentConfigRow(
    string ComponentName,
    bool? RequestedEnabled,
    bool? ActualEnabled,
    string Status);

public sealed partial class SccmSettingsMgmtViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmSettingsMgmtViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmComponentConfigRow> Components { get; } = new();

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

    private bool _showDifferencesOnly;
    public bool ShowDifferencesOnly
    {
        get => _showDifferencesOnly;
        set
        {
            if (SetProperty(ref _showDifferencesOnly, value))
            {
                RefreshCommand.Execute(null);
            }
        }
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
                var requested = agent.Client.RequestedConfig.ComponentClientConfig
                    .GroupBy(c => c.ComponentName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().Enabled, StringComparer.OrdinalIgnoreCase);

                var actual = agent.Client.ActualConfig.ComponentClientConfig
                    .GroupBy(c => c.ComponentName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First().Enabled, StringComparer.OrdinalIgnoreCase);

                var allKeys = requested.Keys.Concat(actual.Keys)
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var list = new List<SccmComponentConfigRow>();
                foreach (var key in allKeys)
                {
                    requested.TryGetValue(key, out var req);
                    actual.TryGetValue(key, out var act);

                    var status = req == act ? "Match" : "Different";
                    if (ShowDifferencesOnly && status == "Match")
                    {
                        continue;
                    }

                    list.Add(new SccmComponentConfigRow(
                        ComponentName: key,
                        RequestedEnabled: req,
                        ActualEnabled: act,
                        Status: status));
                }

                return list;
            }).ConfigureAwait(false);

            foreach (var row in rows)
            {
                Components.Add(row);
            }

            StatusMessage = $"Loaded {Components.Count} component config entries.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load settings: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

