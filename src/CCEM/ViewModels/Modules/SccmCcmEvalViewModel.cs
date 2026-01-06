using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Automation.functions;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record SccmCcmEvalRow(
    string? Id,
    string? Description,
    string? ResultType,
    string? ResultCode,
    string? ResultDetail,
    string? StepDetail);

public sealed partial class SccmCcmEvalViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmCcmEvalViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmCcmEvalRow> Results { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                RunCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private string _lastRun = string.Empty;
    public string LastRun
    {
        get => _lastRun;
        private set => SetProperty(ref _lastRun, value);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected)
        {
            StatusMessage = "Not connected.";
            Results.Clear();
            LastRun = string.Empty;
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Results.Clear();
        LastRun = string.Empty;

        try
        {
            var result = await Task.Run(() =>
            {
                var last = agent.Client.Health.LastCCMEval;
                var items = agent.Client.Health.GetCCMEvalStatus();

                var rows = items.Select(i => new SccmCcmEvalRow(
                    Id: i.ID,
                    Description: i.Description,
                    ResultType: i.ResultType,
                    ResultCode: i.ResultCode,
                    ResultDetail: i.ResultDetail,
                    StepDetail: i.StepDetail)).ToList();

                return (last, rows);
            }).ConfigureAwait(false);

            LastRun = result.last == default ? "Unknown" : result.last.ToString("u");

            foreach (var row in result.rows.OrderBy(r => r.Id))
            {
                Results.Add(row);
            }

            StatusMessage = $"Loaded {Results.Count} results.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load CCMEval status: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task RunAsync()
    {
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = "Starting CCMEval...";

        try
        {
            await Task.Run(() => agent.Client.Health.RunCCMEval()).ConfigureAwait(false);
            StatusMessage = "CCMEval started.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to start CCMEval: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanRun() => !IsLoading;
}

