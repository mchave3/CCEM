using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmServiceWindowsPage : Page
{
    public SccmServiceWindowsViewModel ViewModel { get; }

    public SccmServiceWindowsPage()
    {
        ViewModel = App.GetService<SccmServiceWindowsViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

