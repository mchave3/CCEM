using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmCollectionVariablesPage : Page
{
    public SccmCollectionVariablesViewModel ViewModel { get; }

    public SccmCollectionVariablesPage()
    {
        ViewModel = App.GetService<SccmCollectionVariablesViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

