using CCEM.ViewModels.Modules;

namespace CCEM.Views.Modules;

public sealed partial class SccmToolsPage : Page
{
    public SccmToolsViewModel ViewModel { get; }

    public SccmToolsPage()
    {
        ViewModel = App.GetService<SccmToolsViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.EnsureScriptRootCommand.Execute(null);
        ViewModel.RefreshScriptsCommand.Execute(null);
    }

    private async void RunTool_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        if (button.Tag is not SccmToolRow tool)
        {
            return;
        }

        try
        {
            await ViewModel.RunToolCommand.ExecuteAsync(tool);
        }
        catch
        {
        }
    }
}
