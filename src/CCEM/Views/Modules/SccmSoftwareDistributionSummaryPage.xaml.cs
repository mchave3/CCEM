using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmSoftwareDistributionSummaryPage : Page
{
    public SccmSoftwareDistributionSummaryViewModel ViewModel { get; }

    public SccmSoftwareDistributionSummaryPage()
    {
        ViewModel = App.GetService<SccmSoftwareDistributionSummaryViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

