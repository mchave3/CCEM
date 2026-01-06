using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmPowerSettingsPage : Page
{
    public SccmPowerSettingsViewModel ViewModel { get; }

    public SccmPowerSettingsPage()
    {
        ViewModel = App.GetService<SccmPowerSettingsViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

