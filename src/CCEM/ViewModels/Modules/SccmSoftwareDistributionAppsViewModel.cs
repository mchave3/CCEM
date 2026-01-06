using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Automation.functions;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record SccmApplicationRow(
    string Name,
    string? SoftwareVersion,
    string? Publisher,
    string? InstallState,
    string EvaluationStateText,
    string AllowedActionsText,
    softwaredistribution.CCM_Application Source);

public sealed partial class SccmSoftwareDistributionAppsViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmSoftwareDistributionAppsViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmApplicationRow> Applications { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                InstallSelectedCommand.NotifyCanExecuteChanged();
                RepairSelectedCommand.NotifyCanExecuteChanged();
                UninstallSelectedCommand.NotifyCanExecuteChanged();
                DownloadSelectedCommand.NotifyCanExecuteChanged();
                CancelSelectedCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private SccmApplicationRow? _selectedApplication;
    public SccmApplicationRow? SelectedApplication
    {
        get => _selectedApplication;
        set
        {
            if (SetProperty(ref _selectedApplication, value))
            {
                InstallSelectedCommand.NotifyCanExecuteChanged();
                RepairSelectedCommand.NotifyCanExecuteChanged();
                UninstallSelectedCommand.NotifyCanExecuteChanged();
                DownloadSelectedCommand.NotifyCanExecuteChanged();
                CancelSelectedCommand.NotifyCanExecuteChanged();
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
            Applications.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Applications.Clear();

        try
        {
            var rows = await Task.Run(() =>
            {
                var list = agent.Client.SoftwareDistribution.Applications;
                return list.Select(a => new SccmApplicationRow(
                    Name: a.Name ?? string.Empty,
                    SoftwareVersion: a.SoftwareVersion,
                    Publisher: a.Publisher,
                    InstallState: a.InstallState,
                    EvaluationStateText: a.EvaluationStateText,
                    AllowedActionsText: a.AllowedActions is null ? string.Empty : string.Join(", ", a.AllowedActions),
                    Source: a)).ToList();
            }).ConfigureAwait(false);

            foreach (var row in rows.OrderBy(r => r.Name))
            {
                Applications.Add(row);
            }

            StatusMessage = $"Loaded {Applications.Count} applications.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load applications: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanActOnSelected() => !IsLoading && SelectedApplication is not null;

    [RelayCommand(CanExecute = nameof(CanActOnSelected))]
    private async Task InstallSelectedAsync()
    {
        var selected = SelectedApplication;
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

    [RelayCommand(CanExecute = nameof(CanActOnSelected))]
    private async Task RepairSelectedAsync()
    {
        var selected = SelectedApplication;
        if (selected is null)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = $"Repairing: {selected.Name}";

        try
        {
            await Task.Run(() => selected.Source.Repair()).ConfigureAwait(false);
            StatusMessage = "Repair requested.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Repair failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanActOnSelected))]
    private async Task UninstallSelectedAsync()
    {
        var selected = SelectedApplication;
        if (selected is null)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = $"Uninstalling: {selected.Name}";

        try
        {
            await Task.Run(() => selected.Source.Uninstall()).ConfigureAwait(false);
            StatusMessage = "Uninstall requested.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Uninstall failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanActOnSelected))]
    private async Task DownloadSelectedAsync()
    {
        var selected = SelectedApplication;
        if (selected is null)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = $"Downloading content: {selected.Name}";

        try
        {
            await Task.Run(() => selected.Source.DownloadContents()).ConfigureAwait(false);
            StatusMessage = "Download requested.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Download failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanActOnSelected))]
    private async Task CancelSelectedAsync()
    {
        var selected = SelectedApplication;
        if (selected is null)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = $"Canceling: {selected.Name}";

        try
        {
            await Task.Run(() => selected.Source.Cancel()).ConfigureAwait(false);
            StatusMessage = "Cancel requested.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Cancel failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

