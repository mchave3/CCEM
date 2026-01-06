using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmAgentSettingsPage : Page
{
    public SccmAgentSettingsViewModel ViewModel { get; }

    public SccmAgentSettingsPage()
    {
        ViewModel = App.GetService<SccmAgentSettingsViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

