using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Contracts;
using CCEM.Core.Sccm.Models;
using CommunityToolkit.Mvvm.Input;

namespace CCEM.ViewModels.Sccm;

public partial class InventoryCacheViewModel : SccmViewModelBase
{
    private readonly ISccmClientService _clientService;

    public ObservableCollection<SccmCacheEntry> CacheEntries { get; } = new();

    public InventoryCacheViewModel(ISccmClientService clientService)
    {
        _clientService = clientService;
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsLoading = true;
        Error = null;
        CacheEntries.Clear();

        try
        {
            var connection = CreateConnection();
            var items = await _clientService.GetCacheAsync(connection);
            foreach (var item in items)
            {
                CacheEntries.Add(item);
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
