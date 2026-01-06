using System.Collections.ObjectModel;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record SccmSoftwareStatusRow(
    string Name,
    string? Type,
    string? Publisher,
    string? Status,
    uint PercentComplete,
    uint ErrorCode);

public sealed partial class SccmSoftwareDistributionSummaryViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmSoftwareDistributionSummaryViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmSoftwareStatusRow> Items { get; } = new();

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
            Items.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Items.Clear();

        try
        {
            var rows = await Task.Run(() =>
            {
                var list = agent.Client.SoftwareDistribution.SoftwareSummary;
                return list.Select(i => new SccmSoftwareStatusRow(
                    Name: i.Name ?? string.Empty,
                    Type: i.Type,
                    Publisher: i.Publisher,
                    Status: i.Status,
                    PercentComplete: i.PercentComplete,
                    ErrorCode: i.ErrorCode)).ToList();
            }).ConfigureAwait(false);

            foreach (var row in rows.OrderBy(r => r.Type).ThenBy(r => r.Name))
            {
                Items.Add(row);
            }

            StatusMessage = $"Loaded {Items.Count} items.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load summary: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

