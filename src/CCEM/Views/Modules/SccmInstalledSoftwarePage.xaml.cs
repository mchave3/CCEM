using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmInstalledSoftwarePage : Page
{
    public SccmInstalledSoftwareViewModel ViewModel { get; }

    public SccmInstalledSoftwarePage()
    {
        ViewModel = App.GetService<SccmInstalledSoftwareViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

