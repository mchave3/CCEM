using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmComponentsPage : Page
{
    public SccmComponentsViewModel ViewModel { get; }

    public SccmComponentsPage()
    {
        ViewModel = App.GetService<SccmComponentsViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

