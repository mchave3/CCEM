using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmSettingsMgmtPage : Page
{
    public SccmSettingsMgmtViewModel ViewModel { get; }

    public SccmSettingsMgmtPage()
    {
        ViewModel = App.GetService<SccmSettingsMgmtViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

