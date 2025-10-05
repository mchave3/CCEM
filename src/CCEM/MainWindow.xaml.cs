using CCEM.SCCM.Services;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace CCEM.Views;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    private readonly ISCCMConnectionService _connectionService;

    public MainWindow()
    {
        ViewModel = App.GetService<MainViewModel>();
        _connectionService = App.GetService<ISCCMConnectionService>();

        this.InitializeComponent();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

        var navService = App.GetService<IJsonNavigationService>() as JsonNavigationService;
        if (navService != null)
        {
            navService.Initialize(NavView, NavFrame, NavigationPageMappings.PageDictionary)
                .ConfigureDefaultPage(typeof(HomeLandingPage))
                .ConfigureSettingsPage(typeof(SettingsPage))
                .ConfigureJsonFile("Assets/NavViewMenu/AppData.json", OrderItemsType.None)
                .ConfigureTitleBar(AppTitleBar)
                .ConfigureBreadcrumbBar(BreadCrumbNav, BreadcrumbPageMappings.PageDictionary);
        }

        // Subscribe to connection status changes
        _connectionService.ConnectionStatusChanged += OnConnectionStatusChanged;

        // Initialize connection status
        UpdateConnectionStatus();
    }

    /// <summary>
    /// Handles connection status changes
    /// </summary>
    private void OnConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(UpdateConnectionStatus);
    }

    /// <summary>
    /// Updates the connection status indicator in the title bar
    /// </summary>
    private void UpdateConnectionStatus()
    {
        if (_connectionService.IsConnected)
        {
            ConnectionStatusIcon.Glyph = "\uE8FB"; // CheckMark
            ConnectionStatusIcon.Foreground = new SolidColorBrush(Colors.Green);
            ConnectionStatusText.Text = $"Connected: {_connectionService.ConnectedHostname}";
            ConnectionStatusText.Foreground = new SolidColorBrush(Colors.Green);
        }
        else
        {
            ConnectionStatusIcon.Glyph = "\uE894"; // StatusCircleRing
            ConnectionStatusIcon.Foreground = new SolidColorBrush(Colors.Gray);
            ConnectionStatusText.Text = "Not Connected";
            ConnectionStatusText.Foreground = new SolidColorBrush(Colors.Gray);
        }
    }

    private async void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        await App.Current.ThemeService.SetElementThemeWithoutSaveAsync();
    }

    private void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxTextChangedEvent(sender, args, NavFrame);
    }

    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxQuerySubmittedEvent(sender, args, NavFrame);
    }
}

