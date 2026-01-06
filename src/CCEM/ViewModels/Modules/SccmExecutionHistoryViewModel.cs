using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Automation.functions;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record SccmExecutionHistoryRow(
    string ProgramId,
    string? State,
    DateTime? RunStartTime,
    int? ResultCode,
    string? ResultReason,
    string? UserId,
    string? PackageId,
    softwaredistribution.REG_ExecutionHistory Source);

public sealed partial class SccmExecutionHistoryViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmExecutionHistoryViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmExecutionHistoryRow> Entries { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                DeleteSelectedCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private SccmExecutionHistoryRow? _selectedEntry;
    public SccmExecutionHistoryRow? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            if (SetProperty(ref _selectedEntry, value))
            {
                DeleteSelectedCommand.NotifyCanExecuteChanged();
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
            Entries.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Entries.Clear();

        try
        {
            var rows = await Task.Run(() =>
            {
                var list = agent.Client.SoftwareDistribution.ExecutionHistory;
                return list.Select(e => new SccmExecutionHistoryRow(
                    ProgramId: e._ProgramID,
                    State: e._State,
                    RunStartTime: e._RunStartTime,
                    ResultCode: e.SuccessOrFailureCode,
                    ResultReason: e.SuccessOrFailureReason,
                    UserId: e.UserID,
                    PackageId: e.PackageID,
                    Source: e)).ToList();
            }).ConfigureAwait(false);

            foreach (var row in rows.OrderByDescending(r => r.RunStartTime).ThenBy(r => r.ProgramId))
            {
                Entries.Add(row);
            }

            StatusMessage = $"Loaded {Entries.Count} entries.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load execution history: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private async Task DeleteSelectedAsync()
    {
        var selected = SelectedEntry;
        if (selected is null)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = "Deleting selected entry...";

        try
        {
            await Task.Run(() => selected.Source.Delete()).ConfigureAwait(false);
            StatusMessage = "Deleted.";
            RefreshCommand.Execute(null);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Delete failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanDeleteSelected() => !IsLoading && SelectedEntry is not null;
}

