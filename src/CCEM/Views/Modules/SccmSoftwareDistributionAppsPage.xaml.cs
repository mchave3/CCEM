using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmSoftwareDistributionAppsPage : Page
{
    public SccmSoftwareDistributionAppsViewModel ViewModel { get; }

    public SccmSoftwareDistributionAppsPage()
    {
        ViewModel = App.GetService<SccmSoftwareDistributionAppsViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

