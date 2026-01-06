using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmLogsPage : Page
{
    public SccmLogsViewModel ViewModel { get; }

    public SccmLogsPage()
    {
        ViewModel = App.GetService<SccmLogsViewModel>();
        InitializeComponent();
    }
}

