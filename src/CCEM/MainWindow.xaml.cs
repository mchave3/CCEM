using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CCEM.Views.Modules;
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

    private enum ModuleKind
    {
        Sccm,
        Intune
    }

    private sealed record ModuleNavConfig(string MenuJsonPath, Type DefaultPage);

    private static readonly IReadOnlyDictionary<ModuleKind, ModuleNavConfig> ModuleConfigs = new Dictionary<ModuleKind, ModuleNavConfig>
    {
        { ModuleKind.Sccm, new ModuleNavConfig("Assets/NavViewMenu/Sccm.json", typeof(SccmLandingPage)) },
        { ModuleKind.Intune, new ModuleNavConfig("Assets/NavViewMenu/Intune.json", typeof(IntuneLandingPage)) }
    };

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

    private void SccmButton_Click(object sender, RoutedEventArgs e)
    {
        StartModule(ModuleKind.Sccm);
    }

    private void IntuneButton_Click(object sender, RoutedEventArgs e)
    {
        StartModule(ModuleKind.Intune);
    }

    private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer == ExitNavItem)
        {
            sender.SelectedItem = null;
            ShowModuleSelection();
        }
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
            // Start entry animations.
            InAnimation_Icon.Start();
            InAnimation_Text.Start();

            // Wait before showing loading indicator.
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            LoadingIndicator.Visibility = Visibility.Visible;
        });
    }

    /// <summary>
    /// Switches from the loading screen to the module selection screen.
    /// </summary>
    public void ShowModuleSelection()
    {
        RunOnDispatcher(() =>
        {
            // Hide the loading screen.
            LoadingRoot.Visibility = Visibility.Collapsed;

            // Show module selection.
            ShellRoot.Visibility = Visibility.Collapsed;
            ModuleSelectionRoot.Visibility = Visibility.Visible;
            SetTitleBar(MainContentGrid);
        });
    }

    private void StartModule(ModuleKind module)
    {
        RunOnDispatcher(() =>
        {
            if (!ModuleConfigs.TryGetValue(module, out var config))
            {
                return;
            }

            ModuleSelectionRoot.Visibility = Visibility.Collapsed;
            ShellRoot.Visibility = Visibility.Visible;
            SetTitleBar(AppTitleBar);

            var navService = App.GetService<IJsonNavigationService>() as JsonNavigationService;
            if (navService != null)
            {
                navService.Initialize(NavView, NavFrame, NavigationPageMappings.PageDictionary)
                    .ConfigureDefaultPage(config.DefaultPage)
                    .ConfigureSettingsPage(typeof(SettingsPage))
                    .ConfigureJsonFile(config.MenuJsonPath)
                    .ConfigureTitleBar(AppTitleBar)
                    .ConfigureBreadcrumbBar(BreadCrumbNav, BreadcrumbPageMappings.PageDictionary);
            }

            EnsureExitFooterItem();
        });
    }
    #endregion UI Transitions

    private void EnsureExitFooterItem()
    {
        if (NavView.FooterMenuItemsSource is not null)
        {
            NavView.FooterMenuItemsSource = null;
        }

        if (!NavView.FooterMenuItems.Contains(ExitNavItem))
        {
            NavView.FooterMenuItems.Add(ExitNavItem);
        }
    }

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
