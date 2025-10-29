using System;
using System.Threading;
using System.Threading.Tasks;
using CCEM.Core.Startup;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;

namespace CCEM.Views;

public sealed partial class MainWindow : Window, ISplashScreenHost
{
    private readonly DispatcherQueue _dispatcherQueue;
    private bool _shellPresented;

    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        ViewModel = App.GetService<MainViewModel>();
        this.InitializeComponent();
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

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

    public void ShowSplash()
    {
        _shellPresented = false;
        RunOnDispatcher(() =>
        {
            SplashRoot.Visibility = Visibility.Visible;
            ShellRoot.Visibility = Visibility.Collapsed;
            SplashProgressBar.IsIndeterminate = true;
        });
    }

    public void UpdateStatus(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        RunOnDispatcher(() => SplashStatusText.Text = message);
    }

    public Task EnterShellAsync(CancellationToken cancellationToken = default)
    {
        if (_shellPresented)
        {
            return Task.CompletedTask;
        }

        _shellPresented = true;

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        var completion = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        CancellationTokenRegistration registration = default;
        var hasRegistration = false;

        if (cancellationToken.CanBeCanceled)
        {
            registration = cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken), useSynchronizationContext: false);
            hasRegistration = true;
        }

        RunOnDispatcher(() =>
        {
            SplashProgressBar.IsIndeterminate = false;
            SplashRoot.Visibility = Visibility.Collapsed;
            ShellRoot.Visibility = Visibility.Visible;
            if (hasRegistration)
            {
                registration.Dispose();
            }
            completion.TrySetResult(null);
        });

        return completion.Task;
    }

    private void RunOnDispatcher(Action action)
    {
        if (_dispatcherQueue.HasThreadAccess)
        {
            action();
            return;
        }

        if (!_dispatcherQueue.TryEnqueue(() => action()))
        {
            action();
        }
    }
}
