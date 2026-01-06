using System.Collections.ObjectModel;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record SccmProcessRow(string Name, uint? ProcessId, string? Owner);

public partial class SccmProcessesViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmProcessesViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmProcessRow> Processes { get; } = new();

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
            Processes.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Processes.Clear();

        try
        {
            var rows = await Task.Run(() =>
            {
                var list = agent.Client.Process.ExtProcesses(Reload: true);
                return list.Select(p => new SccmProcessRow(
                    Name: p.Name,
                    ProcessId: p.ProcessId,
                    Owner: p.Owner)).ToList();
            }).ConfigureAwait(false);

            foreach (var row in rows.OrderBy(r => r.Name))
            {
                Processes.Add(row);
            }

            StatusMessage = $"Loaded {Processes.Count} processes.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load processes: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

