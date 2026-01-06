using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed partial class SccmInstallRepairViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmInstallRepairViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

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

    private bool EnsureConnected()
    {
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected)
        {
            StatusMessage = "Not connected.";
            return false;
        }

        return true;
    }

    [RelayCommand]
    private async Task RepairAgentAsync()
    {
        if (!EnsureConnected())
        {
            return;
        }

        var agent = _connectionService.Agent!;
        IsLoading = true;
        StatusMessage = "Repairing SCCM client (msiexec /f)...";

        try
        {
            var ok = await Task.Run(() => agent.Client.AgentActions.RepairAgent()).ConfigureAwait(false);
            StatusMessage = ok ? "Repair started." : "Repair not available (ProductCode missing).";
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

    [RelayCommand]
    private async Task UninstallAgentAsync()
    {
        if (!EnsureConnected())
        {
            return;
        }

        var agent = _connectionService.Agent!;
        IsLoading = true;
        StatusMessage = "Uninstalling SCCM client (msiexec /x)...";

        try
        {
            var ok = await Task.Run(() => agent.Client.AgentActions.UninstallAgent()).ConfigureAwait(false);
            StatusMessage = ok ? "Uninstall requested." : "Uninstall not available (ProductCode missing).";
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

    [RelayCommand]
    private async Task ResetPausedSoftwareDistributionAsync()
    {
        if (!EnsureConnected())
        {
            return;
        }

        var agent = _connectionService.Agent!;
        IsLoading = true;
        StatusMessage = "Resetting paused Software Distribution state...";

        try
        {
            var ok = await Task.Run(() => agent.Client.AgentActions.ResetPausedSWDist()).ConfigureAwait(false);
            StatusMessage = ok ? "Reset done." : "Reset failed.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Reset failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ResetProvisioningModeAsync()
    {
        if (!EnsureConnected())
        {
            return;
        }

        var agent = _connectionService.Agent!;
        IsLoading = true;
        StatusMessage = "Resetting provisioning mode...";

        try
        {
            var ok = await Task.Run(() => agent.Client.AgentActions.ResetProvisioningMode()).ConfigureAwait(false);
            StatusMessage = ok ? "Reset done." : "Reset failed.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Reset failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

