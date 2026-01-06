using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmAdvertisementsPage : Page
{
    public SccmAdvertisementsViewModel ViewModel { get; }

    public SccmAdvertisementsPage()
    {
        ViewModel = App.GetService<SccmAdvertisementsViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

