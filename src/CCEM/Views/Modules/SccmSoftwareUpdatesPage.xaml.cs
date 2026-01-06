using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmSoftwareUpdatesPage : Page
{
    public SccmSoftwareUpdatesViewModel ViewModel { get; }

    public SccmSoftwareUpdatesPage()
    {
        ViewModel = App.GetService<SccmSoftwareUpdatesViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

