using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmServicesPage : Page
{
    public SccmServicesViewModel ViewModel { get; }

    public SccmServicesPage()
    {
        ViewModel = App.GetService<SccmServicesViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

