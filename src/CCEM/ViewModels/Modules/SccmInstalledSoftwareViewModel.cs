using System.Collections.ObjectModel;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record SccmInstalledSoftwareRow(string DisplayName, string Version, string Publisher, string InstallDate);

public sealed partial class SccmInstalledSoftwareViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmInstalledSoftwareViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmInstalledSoftwareRow> Software { get; } = new();

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
            Software.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Software.Clear();

        try
        {
            var rows = await Task.Run(() =>
            {
                var list = agent.Client.Inventory.InstalledSoftware;
                return list.Select(s => new SccmInstalledSoftwareRow(
                    DisplayName: s.ARPDisplayName ?? s.ProductName ?? string.Empty,
                    Version: s.ProductVersion ?? string.Empty,
                    Publisher: s.Publisher ?? string.Empty,
                    InstallDate: s.InstallDate ?? string.Empty)).ToList();
            }).ConfigureAwait(false);

            foreach (var row in rows.Where(r => !string.IsNullOrWhiteSpace(r.DisplayName)).OrderBy(r => r.DisplayName))
            {
                Software.Add(row);
            }

            StatusMessage = $"Loaded {Software.Count} entries.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load installed software: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

