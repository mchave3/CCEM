namespace CCEM.Views.Intune;

public sealed partial class ComingSoonPage : Page
{
    public ComingSoonPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        PlaceholderHost.Navigate(typeof(CCEM.Views.Shared.PlaceholderPage), new CCEM.Views.Shared.PlaceholderPage.PlaceholderNavigationContext(
            "Intune workspace",
            "Microsoft Intune integration is coming soon. The navigation structure is ready for the future Intune experience.",
            "\uE753"));
    }
}
