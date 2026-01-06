using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmWmiBrowserPage : Page
{
    public SccmWmiBrowserViewModel ViewModel { get; }

    public SccmWmiBrowserPage()
    {
        ViewModel = App.GetService<SccmWmiBrowserViewModel>();
        InitializeComponent();
    }
}

