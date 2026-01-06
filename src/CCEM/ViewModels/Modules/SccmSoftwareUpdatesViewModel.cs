using System.Collections.ObjectModel;
using CCEM.Services;
using CCEM.Core.Sccm.Automation.functions;

namespace CCEM.ViewModels.Modules;

public sealed record SccmSoftwareUpdateRow(
    string Name,
    string? Publisher,
    uint? ComplianceState,
    uint? EvaluationState,
    string EvaluationStateText,
    DateTime? Deadline,
    DateTime? RestartDeadline,
    uint? PercentComplete,
    string? ErrorCodeText,
    string UpdateId,
    softwareupdates.CCM_SoftwareUpdate Source);

public sealed partial class SccmSoftwareUpdatesViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmSoftwareUpdatesViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmSoftwareUpdateRow> Updates { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                InstallSelectedCommand.NotifyCanExecuteChanged();
                InstallAllRequiredCommand.NotifyCanExecuteChanged();
                InstallAllApprovedCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private bool _showAll;
    public bool ShowAll
    {
        get => _showAll;
        set
        {
            if (SetProperty(ref _showAll, value))
            {
                RefreshCommand.Execute(null);
            }
        }
    }

    private SccmSoftwareUpdateRow? _selectedUpdate;
    public SccmSoftwareUpdateRow? SelectedUpdate
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
                if (!ShowAll)
                {
                    list = list.Where(u => u.ComplianceState == 0).ToList();
                }

                return list.Select(u => new SccmSoftwareUpdateRow(
                    Name: u.Name,
                    Publisher: u.Publisher,
                    ComplianceState: u.ComplianceState,
                    EvaluationState: u.EvaluationState,
                    EvaluationStateText: u.EvaluationStateText,
                    Deadline: u.Deadline,
                    RestartDeadline: u.RestartDeadline,
                    PercentComplete: u.PercentComplete,
                    ErrorCodeText: u.ErrorCodeText,
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
        var agent = _connectionService.Agent;
        var update = SelectedUpdate;
        if (agent is null || !agent.isConnected || update is null)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = $"Installing: {update.Name}";

        try
        {
            await Task.Run(() => update.Source.Install()).ConfigureAwait(false);
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

    [RelayCommand(CanExecute = nameof(CanInstallAll))]
    private async Task InstallAllRequiredAsync()
    {
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = "Installing all required updates...";

        try
        {
            await Task.Run(() => agent.Client.SoftwareUpdates.InstallAllRequiredUpdates()).ConfigureAwait(false);
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

    [RelayCommand(CanExecute = nameof(CanInstallAll))]
    private async Task InstallAllApprovedAsync()
    {
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = "Installing all approved updates...";

        try
        {
            await Task.Run(() => agent.Client.SoftwareUpdates.InstallAllApprovedUpdates()).ConfigureAwait(false);
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

    private bool CanInstallAll() => !IsLoading;
}
