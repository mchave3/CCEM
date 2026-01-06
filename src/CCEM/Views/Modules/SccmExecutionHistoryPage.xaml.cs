using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmExecutionHistoryPage : Page
{
    public SccmExecutionHistoryViewModel ViewModel { get; }

    public SccmExecutionHistoryPage()
    {
        ViewModel = App.GetService<SccmExecutionHistoryViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

