using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmAllUpdatesPage : Page
{
    public SccmAllUpdatesViewModel ViewModel { get; }

    public SccmAllUpdatesPage()
    {
        ViewModel = App.GetService<SccmAllUpdatesViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

