using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed partial class SccmAgentActionsViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmAgentActionsViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private async Task RunAsync(string label, Func<bool> action)
    {
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected)
        {
            StatusMessage = "Not connected.";
            return;
        }

        IsBusy = true;
        StatusMessage = $"{label}...";

        try
        {
            var ok = await Task.Run(action).ConfigureAwait(false);
            StatusMessage = ok ? $"{label}: OK" : $"{label}: failed";
        }
        catch (Exception ex)
        {
            StatusMessage = $"{label}: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task HardwareInventoryDeltaAsync() => RunAsync(
        "Hardware inventory (delta)",
        () => _connectionService.Agent!.Client.AgentActions.HardwareInventory(false));

    [RelayCommand]
    private Task HardwareInventoryFullAsync() => RunAsync(
        "Hardware inventory (full)",
        () => _connectionService.Agent!.Client.AgentActions.HardwareInventory(true));

    [RelayCommand]
    private Task SoftwareInventoryDeltaAsync() => RunAsync(
        "Software inventory (delta)",
        () => _connectionService.Agent!.Client.AgentActions.SoftwareInventory(false));

    [RelayCommand]
    private Task SoftwareInventoryFullAsync() => RunAsync(
        "Software inventory (full)",
        () => _connectionService.Agent!.Client.AgentActions.SoftwareInventory(true));

    [RelayCommand]
    private Task MachinePolicyRequestAsync() => RunAsync(
        "Machine policy (request assignments)",
        () => _connectionService.Agent!.Client.AgentActions.RequestMachinePolicyAssignments());

    [RelayCommand]
    private Task MachinePolicyEvaluateAsync() => RunAsync(
        "Machine policy (evaluate assignments)",
        () => _connectionService.Agent!.Client.AgentActions.EvaluateMachinePolicyAssignments());

    [RelayCommand]
    private Task UserPolicyRequestAsync() => RunAsync(
        "User policy (request assignments)",
        () => _connectionService.Agent!.Client.AgentActions.RequestUserAssignments());

    [RelayCommand]
    private Task UserPolicyEvaluateAsync() => RunAsync(
        "User policy (evaluate assignments)",
        () => _connectionService.Agent!.Client.AgentActions.EvaluateUserPolicies());

    [RelayCommand]
    private Task SoftwareUpdatesEvaluateCycleAsync() => RunAsync(
        "Software updates (assignment evaluation cycle)",
        () => _connectionService.Agent!.Client.AgentActions.SoftwareUpdatesAgentAssignmentEvaluationCycle());

    [RelayCommand]
    private Task SoftwareUpdatesScanAsync() => RunAsync(
        "Software updates (force scan)",
        () => _connectionService.Agent!.Client.AgentActions.ForceUpdateScan());

    [RelayCommand]
    private Task ResetPolicySoftAsync() => RunAsync(
        "Reset policy (soft)",
        () => _connectionService.Agent!.Client.AgentActions.ResetPolicy(false));

    [RelayCommand]
    private Task ResetPolicyHardAsync() => RunAsync(
        "Reset policy (hard)",
        () => _connectionService.Agent!.Client.AgentActions.ResetPolicy(true));
}

