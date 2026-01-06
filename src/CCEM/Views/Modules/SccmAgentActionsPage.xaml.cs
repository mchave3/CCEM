using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmAgentActionsPage : Page
{
    public SccmAgentActionsViewModel ViewModel { get; }

    public SccmAgentActionsPage()
    {
        ViewModel = App.GetService<SccmAgentActionsViewModel>();
        InitializeComponent();
    }
}

