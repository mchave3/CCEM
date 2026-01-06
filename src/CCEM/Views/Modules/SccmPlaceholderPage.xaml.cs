namespace CCEM.Views.Modules;

public sealed partial class SccmPlaceholderPage : Page
{
    public SccmPlaceholderPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        HeaderText.Text = e.Parameter?.ToString() ?? "SCCM";
    }
}

