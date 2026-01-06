using System.Collections.ObjectModel;
using CCEM.Services;
using CCEM.Core.Sccm.Automation.functions;

namespace CCEM.ViewModels.Modules;

public sealed record SccmServiceRow(string Name, string DisplayName, string State, string StartName);

public partial class SccmServicesViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmServicesViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmServiceRow> Services { get; } = new();

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
            Services.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Services.Clear();

        try
        {
            var rows = await Task.Run(() =>
            {
                var list = agent.Client.Services.Win32_Services;
                return list.Select(s => new SccmServiceRow(
                    Name: s.Name,
                    DisplayName: s.DisplayName,
                    State: s.State,
                    StartName: s.StartName)).ToList();
            }).ConfigureAwait(false);

            foreach (var row in rows.OrderBy(r => r.DisplayName))
            {
                Services.Add(row);
            }

            StatusMessage = $"Loaded {Services.Count} services.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load services: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
