using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Automation.functions;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record SccmAllUpdateRow(
    string Name,
    string? Publisher,
    uint? ComplianceState,
    uint? EvaluationState,
    string EvaluationStateText,
    string UpdateId,
    softwareupdates.CCM_SoftwareUpdate Source);

public sealed partial class SccmAllUpdatesViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmAllUpdatesViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmAllUpdateRow> Updates { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                InstallSelectedCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private SccmAllUpdateRow? _selectedUpdate;
    public SccmAllUpdateRow? SelectedUpdate
    {
        get => _selectedUpdate;
        set
        {
            if (SetProperty(ref _selectedUpdate, value))
            {
                InstallSelectedCommand.NotifyCanExecuteChanged();
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
            Updates.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Updates.Clear();

        try
        {
            var rows = await Task.Run(() =>
            {
                var list = agent.Client.SoftwareUpdates.GetSoftwareUpdate(true);
                return list.Select(u => new SccmAllUpdateRow(
                    Name: u.Name,
                    Publisher: u.Publisher,
                    ComplianceState: u.ComplianceState,
                    EvaluationState: u.EvaluationState,
                    EvaluationStateText: u.EvaluationStateText,
                    UpdateId: u.UpdateID,
                    Source: u)).ToList();
            }).ConfigureAwait(false);

            foreach (var row in rows.OrderBy(r => r.Name))
            {
                Updates.Add(row);
            }

            StatusMessage = $"Loaded {Updates.Count} updates.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load updates: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanInstallSelected))]
    private async Task InstallSelectedAsync()
    {
        var selected = SelectedUpdate;
        if (selected is null)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = $"Installing: {selected.Name}";

        try
        {
            await Task.Run(() => selected.Source.Install()).ConfigureAwait(false);
            StatusMessage = "Install requested.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Install failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanInstallSelected() => !IsLoading && SelectedUpdate is not null;
}

