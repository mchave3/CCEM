using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmInstallRepairPage : Page
{
    public SccmInstallRepairViewModel ViewModel { get; }

    public SccmInstallRepairPage()
    {
        ViewModel = App.GetService<SccmInstallRepairViewModel>();
        InitializeComponent();
    }
}

