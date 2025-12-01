using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Contracts;
using CCEM.Core.Sccm.Models;
using CommunityToolkit.Mvvm.Input;

namespace CCEM.ViewModels.Sccm;

public partial class OperationsProcessesViewModel : SccmViewModelBase
{
    private readonly ISccmClientService _clientService;

    public ObservableCollection<SccmProcessInfo> Processes { get; } = new();

    public OperationsProcessesViewModel(ISccmClientService clientService)
    {
        _clientService = clientService;
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsLoading = true;
        Error = null;
        Processes.Clear();

        try
        {
            var connection = CreateConnection();
            var items = await _clientService.GetProcessesAsync(connection);
            foreach (var item in items)
            {
                Processes.Add(item);
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
