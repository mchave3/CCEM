using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmEventMonitoringPage : Page
{
    public SccmEventMonitoringViewModel ViewModel { get; }

    public SccmEventMonitoringPage()
    {
        ViewModel = App.GetService<SccmEventMonitoringViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

