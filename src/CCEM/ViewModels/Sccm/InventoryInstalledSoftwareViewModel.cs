using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Contracts;
using CCEM.Core.Sccm.Models;
using CommunityToolkit.Mvvm.Input;

namespace CCEM.ViewModels.Sccm;

public partial class InventoryInstalledSoftwareViewModel : SccmViewModelBase
{
    private readonly ISccmClientService _clientService;

    public ObservableCollection<SccmInstalledSoftware> Items { get; } = new();

    public InventoryInstalledSoftwareViewModel(ISccmClientService clientService)
    {
        _clientService = clientService;
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsLoading = true;
        Error = null;
        Items.Clear();

        try
        {
            var connection = CreateConnection();
            var items = await _clientService.GetInstalledSoftwareAsync(connection);
            foreach (var item in items)
            {
                Items.Add(item);
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
