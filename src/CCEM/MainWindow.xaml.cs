using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;

namespace CCEM.Views;

public sealed partial class MainWindow : Window
{
    private readonly DispatcherQueue _dispatcherQueue;

    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        ViewModel = App.GetService<MainViewModel>();
        this.InitializeComponent();
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(MainContentGrid);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
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

    /// <summary>
    /// Plays the entry animations for the loading screen
    /// </summary>
    public async Task DoEntryAnimationAsync()
    {
        await RunOnDispatcherAsync(async () =>
        {
            // Start both animations simultaneously
            InAnimation_Icon.Start();
            InAnimation_Text.Start();

            // Wait for animations to complete (10000ms duration)
            await Task.Delay(10000);
        });
    }

    /// <summary>
    /// Switches from the loading screen to the main interface
    /// </summary>
    public void SwitchToInterface()
    {
        RunOnDispatcher(() =>
        {
            // Hide the loading screen
            LoadingRoot.Visibility = Visibility.Collapsed;

            // Show the main shell
            ShellRoot.Visibility = Visibility.Visible;

            // Update title bar
            SetTitleBar(AppTitleBar);

            // Initialize navigation service
            var navService = App.GetService<IJsonNavigationService>() as JsonNavigationService;
            if (navService != null)
            {
                navService.Initialize(NavView, NavFrame, NavigationPageMappings.PageDictionary)
                    .ConfigureDefaultPage(typeof(HomeLandingPage))
                    .ConfigureSettingsPage(typeof(SettingsPage))
                    .ConfigureJsonFile("Assets/NavViewMenu/AppData.json")
                    .ConfigureTitleBar(AppTitleBar)
                    .ConfigureBreadcrumbBar(BreadCrumbNav, BreadcrumbPageMappings.PageDictionary);
            }
        });
    }

    private void RunOnDispatcher(Action action)
    {
        if (_dispatcherQueue.HasThreadAccess)
        {
            action();
            return;
        }

        _ = _dispatcherQueue.TryEnqueue(() => action());
    }

    private async Task RunOnDispatcherAsync(Func<Task> action)
    {
        if (_dispatcherQueue.HasThreadAccess)
        {
            await action();
        }
        else
        {
            var tcs = new TaskCompletionSource<bool>();
            _dispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    await action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            await tcs.Task;
        }
    }
}
