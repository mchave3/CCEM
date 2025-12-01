using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Contracts;
using CCEM.Core.Sccm.Models;
using CommunityToolkit.Mvvm.Input;

namespace CCEM.ViewModels.Sccm;

public partial class InventoryComponentsViewModel : SccmViewModelBase
{
    private readonly ISccmClientService _clientService;

    public ObservableCollection<SccmComponentStatus> Components { get; } = new();

    public InventoryComponentsViewModel(ISccmClientService clientService)
    {
        _clientService = clientService;
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsLoading = true;
        Error = null;
        Components.Clear();

        try
        {
            var connection = CreateConnection();
            var items = await _clientService.GetComponentsAsync(connection);
            foreach (var item in items)
            {
                Components.Add(item);
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
