using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Automation.policy;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record SccmServiceWindowRow(
    string Scope,
    string? ServiceWindowId,
    uint? ServiceWindowType,
    string? Schedules);

public sealed partial class SccmServiceWindowsViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmServiceWindowsViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmServiceWindowRow> ServiceWindows { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                CreateCommand.NotifyCanExecuteChanged();
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

    private int _scopeIndex;
    public int ScopeIndex
    {
        get => _scopeIndex;
        set
        {
            if (SetProperty(ref _scopeIndex, value))
            {
                DeleteSelectedCommand.NotifyCanExecuteChanged();
                RefreshCommand.Execute(null);
            }
        }
    }

    private string _newSchedules = string.Empty;
    public string NewSchedules
    {
        get => _newSchedules;
        set
        {
            if (SetProperty(ref _newSchedules, value))
            {
                CreateCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private int _newServiceWindowType = 1;
    public int NewServiceWindowType
    {
        get => _newServiceWindowType;
        set => SetProperty(ref _newServiceWindowType, value);
    }

    private SccmServiceWindowRow? _selectedServiceWindow;
    public SccmServiceWindowRow? SelectedServiceWindow
    {
        get => _selectedServiceWindow;
        set
        {
            if (SetProperty(ref _selectedServiceWindow, value))
            {
                DeleteSelectedCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string ScopeText => ScopeIndex == 1 ? "Actual Config" : "Requested Config";

    [RelayCommand]
    private async Task RefreshAsync()
    {
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected)
        {
            StatusMessage = "Not connected.";
            ServiceWindows.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        ServiceWindows.Clear();

        try
        {
            var scopeText = ScopeText;
            var rows = await Task.Run(() =>
            {
                if (ScopeIndex == 1)
                {
                    var list = agent.Client.ActualConfig.ServiceWindow;
                    return list.Select(sw => new SccmServiceWindowRow(
                        Scope: scopeText,
                        ServiceWindowId: sw.ServiceWindowID,
                        ServiceWindowType: sw.ServiceWindowType,
                        Schedules: sw.Schedules)).ToList();
                }
                else
                {
                    var list = agent.Client.RequestedConfig.ServiceWindow;
                    return list.Select(sw => new SccmServiceWindowRow(
                        Scope: scopeText,
                        ServiceWindowId: sw.ServiceWindowID,
                        ServiceWindowType: sw.ServiceWindowType,
                        Schedules: sw.Schedules)).ToList();
                }
            }).ConfigureAwait(false);

            foreach (var row in rows.OrderBy(r => r.ServiceWindowType).ThenBy(r => r.ServiceWindowId))
            {
                ServiceWindows.Add(row);
            }

            StatusMessage = $"Loaded {ServiceWindows.Count} service windows ({scopeText}).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load service windows: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task CreateAsync()
    {
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = "Creating service window...";

        try
        {
            var schedules = NewSchedules.Trim();
            var type = (uint)Math.Clamp(NewServiceWindowType, 1, 6);

            string? id = await Task.Run(() =>
            {
                return ScopeIndex == 1
                    ? agent.Client.ActualConfig.CreateServiceWindow(schedules, type)
                    : agent.Client.RequestedConfig.CreateServiceWindow(schedules, type);
            }).ConfigureAwait(false);

            StatusMessage = id is null ? "Create failed." : $"Created: {id}";
            RefreshCommand.Execute(null);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Create failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanCreate() => !IsLoading && !string.IsNullOrWhiteSpace(NewSchedules);

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private async Task DeleteSelectedAsync()
    {
        var agent = _connectionService.Agent;
        var selected = SelectedServiceWindow;
        if (agent is null || !agent.isConnected || selected is null)
        {
            return;
        }

        if (ScopeIndex == 1)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = "Deleting service window...";

        try
        {
            var id = selected.ServiceWindowId;
            if (string.IsNullOrWhiteSpace(id))
            {
                StatusMessage = "Invalid ServiceWindowID.";
                return;
            }

            await Task.Run(() => agent.Client.RequestedConfig.DeleteServiceWindow(id)).ConfigureAwait(false);
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

    private bool CanDeleteSelected() => !IsLoading && ScopeIndex == 0 && SelectedServiceWindow is not null;
}

