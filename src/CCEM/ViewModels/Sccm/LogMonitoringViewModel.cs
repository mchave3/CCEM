using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Contracts;
using CommunityToolkit.Mvvm.Input;

namespace CCEM.ViewModels.Sccm;

public partial class LogMonitoringViewModel : SccmViewModelBase
{
    private readonly ILogCaptureService _logService;

    [ObservableProperty]
    private string _logPath = @"C:\Windows\CCM\Logs\ccmexec.log";

    public ObservableCollection<string> Lines { get; } = new();

    public LogMonitoringViewModel(ILogCaptureService logService)
    {
        _logService = logService;
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsLoading = true;
        Error = null;
        Lines.Clear();

        try
        {
            var connection = CreateConnection();
            await foreach (var line in _logService.TailAsync(connection, LogPath, 200))
            {
                Lines.Add(line);
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
