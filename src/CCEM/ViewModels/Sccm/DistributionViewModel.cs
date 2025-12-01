using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Contracts;
using CCEM.Core.Sccm.Models;
using CommunityToolkit.Mvvm.Input;

namespace CCEM.ViewModels.Sccm;

public partial class DistributionViewModel : SccmViewModelBase
{
    private readonly ISccmSoftwareDistributionService _service;

    public ObservableCollection<SccmAdvertisement> Advertisements { get; } = new();
    public ObservableCollection<SccmAdvertisement> ExecutionHistory { get; } = new();

    public DistributionViewModel(ISccmSoftwareDistributionService service)
    {
        _service = service;
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsLoading = true;
        Error = null;
        Advertisements.Clear();
        ExecutionHistory.Clear();

        try
        {
            var connection = CreateConnection();
            var ads = await _service.GetAdvertisementsAsync(connection);
            foreach (var ad in ads)
            {
                Advertisements.Add(ad);
            }

            var history = await _service.GetExecutionHistoryAsync(connection);
            foreach (var item in history)
            {
                ExecutionHistory.Add(item);
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
