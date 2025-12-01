using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Contracts;
using CCEM.Core.Sccm.Models;
using CommunityToolkit.Mvvm.Input;

namespace CCEM.ViewModels.Sccm;

public partial class InventoryEvaluationViewModel : SccmViewModelBase
{
    private readonly ISccmClientService _clientService;

    public ObservableCollection<SccmEvaluationStatus> Evaluations { get; } = new();

    public InventoryEvaluationViewModel(ISccmClientService clientService)
    {
        _clientService = clientService;
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsLoading = true;
        Error = null;
        Evaluations.Clear();

        try
        {
            var connection = CreateConnection();
            var items = await _clientService.GetEvaluationsAsync(connection);
            foreach (var item in items)
            {
                Evaluations.Add(item);
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
