using System.Collections.ObjectModel;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record SccmEventLogRow(
    DateTime? TimeCreated,
    int? Id,
    string? Level,
    string? Message);

public sealed partial class SccmEventMonitoringViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmEventMonitoringViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
        _maxEvents = 50;
    }

    public ObservableCollection<SccmEventLogRow> Events { get; } = new();

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

    private int _maxEvents;
    public int MaxEvents
    {
        get => _maxEvents;
        set => SetProperty(ref _maxEvents, Math.Clamp(value, 1, 500));
    }

    private string _filterText = string.Empty;
    public string FilterText
    {
        get => _filterText;
        set
        {
            if (SetProperty(ref _filterText, value))
            {
                RefreshCommand.Execute(null);
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
            Events.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Events.Clear();

        try
        {
            var rows = await Task.Run(() =>
            {
                var max = MaxEvents;
                var script = $"Get-WinEvent -LogName 'Microsoft-Windows-CCM/Operational' -MaxEvents {max} | Select-Object TimeCreated, Id, LevelDisplayName, Message";
                var objs = agent.Client.GetObjectsFromPS(script, true);

                var filter = FilterText?.Trim();
                var list = new List<SccmEventLogRow>();
                foreach (var o in objs)
                {
                    var time = o.Properties["TimeCreated"]?.Value as DateTime?;
                    var idObj = o.Properties["Id"]?.Value;
                    int? id = idObj is null ? null : Convert.ToInt32(idObj);
                    var level = o.Properties["LevelDisplayName"]?.Value?.ToString();
                    var message = o.Properties["Message"]?.Value?.ToString();

                    if (!string.IsNullOrEmpty(filter))
                    {
                        if (message is null || !message.Contains(filter, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }

                    list.Add(new SccmEventLogRow(time, id, level, message));
                }

                return list;
            }).ConfigureAwait(false);

            foreach (var row in rows.OrderByDescending(r => r.TimeCreated))
            {
                Events.Add(row);
            }

            StatusMessage = $"Loaded {Events.Count} events.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load events: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

