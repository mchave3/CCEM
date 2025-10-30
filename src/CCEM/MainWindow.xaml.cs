using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;

namespace CCEM.Views;

public sealed partial class MainWindow : Window
{
    /// <summary>
    /// The dispatcher queue for the main UI thread.
    /// </summary>
    private readonly DispatcherQueue _dispatcherQueue;

    /// <summary>
    /// The main view model for this window.
    /// </summary>
    public MainViewModel ViewModel { get; }

    #region Application Initialization
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        // Get the main view model from the service provider.
        ViewModel = App.GetService<MainViewModel>();

        // Initialize XAML components.
        this.InitializeComponent();

        // Initialize dispatcher queue.
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        // Configure title bar.
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(MainContentGrid);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
    }
    #endregion Application Initialization

    #region Event Handlers
    /// <summary>
    /// Handles the Click event of the ThemeButton control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        await App.Current.ThemeService.SetElementThemeWithoutSaveAsync();
    }

    /// <summary>
    /// Handles the TextChanged event of the AutoSuggestBox control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxTextChangedEvent(sender, args, NavFrame);
    }

    /// <summary>
    /// Handles the QuerySubmitted event of the AutoSuggestBox control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        AutoSuggestBoxHelper.OnITitleBarAutoSuggestBoxQuerySubmittedEvent(sender, args, NavFrame);
    }
    #endregion Event Handlers

    #region UI Transitions
    /// <summary>
    /// Plays the entry animations for the loading screen.
    /// </summary>
    public async Task DoEntryAnimationAsync()
    {
        await RunOnDispatcherAsync(async () =>
        {
            // Trigger the same entry animations used in UniGetUI.
            InAnimation_Icon.Start();
            InAnimation_Text.Start();

            // Delay showing the loading indicator for better UX.
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            LoadingIndicator.Visibility = Visibility.Visible;
        });
    }

    /// <summary>
    /// Switches from the loading screen to the main interface.
    /// </summary>
    public void SwitchToInterface()
    {
        RunOnDispatcher(() =>
        {
            // Hide the loading screen.
            LoadingRoot.Visibility = Visibility.Collapsed;

            // Show the main shell.
            ShellRoot.Visibility = Visibility.Visible;

            // Update title bar.
            SetTitleBar(AppTitleBar);

            // Initialize navigation service.
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
    #endregion UI Transitions

    #region Dispatcher Helpers
    /// <summary>
    /// Runs the specified action on the UI dispatcher thread.
    /// </summary>
    /// <param name="action"></param>
    private void RunOnDispatcher(Action action)
    {
        if (_dispatcherQueue.HasThreadAccess)
        {
            action();
            return;
        }

        _ = _dispatcherQueue.TryEnqueue(() => action());
    }

    /// <summary>
    /// Runs the specified asynchronous action on the UI dispatcher thread.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    private async Task RunOnDispatcherAsync(Func<Task> action)
    {
        // If we're already on the dispatcher thread, run the action directly.
        if (_dispatcherQueue.HasThreadAccess)
        {
            await action();
        }
        // If we're not on the dispatcher thread, enqueue the action and await its completion.
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
    #endregion Dispatcher Helpers
}
