using System.Collections.ObjectModel;
using CCEM.Services;
using CCEM.Core.Sccm.Automation.functions;

namespace CCEM.ViewModels.Modules;

public sealed record SccmCacheRow(
    string ContentId,
    string ContentVer,
    uint? ContentSize,
    string ContentType,
    DateTime? LastReferenced,
    string Location,
    uint? ReferenceCount,
    bool? PeerCaching,
    swcache.CacheInfoEx Source);

public sealed partial class SccmCacheViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmCacheViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmCacheRow> Items { get; } = new();

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

    private SccmCacheRow? _selectedItem;
    public SccmCacheRow? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
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
            Items.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Items.Clear();

        try
        {
            var rows = await Task.Run(() =>
            {
                var list = agent.Client.SWCache.CachedContent;
                return list.Select(ci => new SccmCacheRow(
                    ContentId: ci.ContentId,
                    ContentVer: ci.ContentVer,
                    ContentSize: ci.ContentSize,
                    ContentType: ci.ContentType,
                    LastReferenced: ci.LastReferenced,
                    Location: ci.Location,
                    ReferenceCount: ci.ReferenceCount,
                    PeerCaching: ci.PeerCaching,
                    Source: ci)).ToList();
            }).ConfigureAwait(false);

            foreach (var row in rows.OrderByDescending(r => r.LastReferenced ?? DateTime.MinValue))
            {
                Items.Add(row);
            }

            StatusMessage = $"Loaded {Items.Count} cache entries.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load cache: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CleanupOrphansAsync()
    {
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected)
        {
            StatusMessage = "Not connected.";
            return;
        }

        IsLoading = true;
        StatusMessage = "Cleaning up orphaned cache items...";

        try
        {
            var result = await Task.Run(() => agent.Client.SWCache.CleanupOrphanedCacheItems()).ConfigureAwait(false);
            StatusMessage = string.IsNullOrWhiteSpace(result) ? "Cleanup complete." : result.Trim();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Cleanup failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private async Task DeleteSelectedAsync()
    {
        var item = SelectedItem;
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected || item is null)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = $"Deleting cache entry {item.ContentId}...";

        try
        {
            await Task.Run(() => item.Source.Delete()).ConfigureAwait(false);
            Items.Remove(item);
            SelectedItem = null;
            StatusMessage = "Deleted.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Delete failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            DeleteSelectedCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanDeleteSelected() => !IsLoading && SelectedItem is not null;
}
