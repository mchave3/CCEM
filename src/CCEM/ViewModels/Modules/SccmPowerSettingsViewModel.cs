using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Automation.functions;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record SccmPowerSettingRow(
    string? Name,
    string? Guid,
    string? Unit,
    string? ACValue,
    string? DCValue);

public sealed partial class SccmPowerSettingsViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmPowerSettingsViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmPowerSettingRow> Settings { get; } = new();

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
            Settings.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Settings.Clear();

        try
        {
            var rows = await Task.Run(() =>
            {
                var list = agent.Client.Inventory.PowerSettings(true);
                return list.Select(s => new SccmPowerSettingRow(
                    Name: s.Name,
                    Guid: s.GUID,
                    Unit: s.UnitSpecifier,
                    ACValue: s.ACDisplayvalue,
                    DCValue: s.DCDisplayvalue)).ToList();
            }).ConfigureAwait(false);

            foreach (var row in rows.OrderBy(r => r.Name))
            {
                Settings.Add(row);
            }

            StatusMessage = $"Loaded {Settings.Count} settings.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load power settings: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

