using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmProcessesPage : Page
{
    public SccmProcessesViewModel ViewModel { get; }

    public SccmProcessesPage()
    {
        ViewModel = App.GetService<SccmProcessesViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

