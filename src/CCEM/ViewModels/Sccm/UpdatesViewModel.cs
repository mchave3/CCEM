using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Contracts;
using CCEM.Core.Sccm.Models;
using CommunityToolkit.Mvvm.Input;

namespace CCEM.ViewModels.Sccm;

public partial class UpdatesViewModel : SccmViewModelBase
{
    private readonly ISccmUpdateService _updateService;

    public ObservableCollection<SccmUpdateInfo> Updates { get; } = new();

    public UpdatesViewModel(ISccmUpdateService updateService)
    {
        _updateService = updateService;
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsLoading = true;
        Error = null;
        Updates.Clear();

        try
        {
            var connection = CreateConnection();
            var items = await _updateService.GetUpdatesAsync(connection);
            foreach (var item in items)
            {
                Updates.Add(item);
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
