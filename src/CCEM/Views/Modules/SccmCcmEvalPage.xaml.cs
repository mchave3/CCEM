using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmCcmEvalPage : Page
{
    public SccmCcmEvalViewModel ViewModel { get; }

    public SccmCcmEvalPage()
    {
        ViewModel = App.GetService<SccmCcmEvalViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

