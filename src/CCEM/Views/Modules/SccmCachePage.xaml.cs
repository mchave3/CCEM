using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmCachePage : Page
{
    public SccmCacheViewModel ViewModel { get; }

    public SccmCachePage()
    {
        ViewModel = App.GetService<SccmCacheViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.RefreshCommand.Execute(null);
    }
}

