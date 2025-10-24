using System.Threading.Tasks;
using CCEM.Core.Velopack.Models;
using CCEM.Core.Velopack.Services;
using Velopack;

namespace CCEM;

public partial class App : Application
{
    public new static App Current => (App)Application.Current;
    public static Window MainWindow = Window.Current;
    public static IntPtr Hwnd => WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
    public IServiceProvider Services { get; }
    public IJsonNavigationService NavService => GetService<IJsonNavigationService>();
    public IThemeService ThemeService => GetService<IThemeService>();

    public static T GetService<T>() where T : class
    {
        if ((App.Current as App)!.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public App()
    {
        VelopackApp.Build().Run();
        ConfigureApplicationLogger();
        UnhandledException += (s, e) => Logger?.Error(e.Exception, "UnhandledException");
        Services = ConfigureServices();

        this.InitializeComponent();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IJsonNavigationService, JsonNavigationService>();
        services.AddSingleton<IVelopackUpdateService>(_ =>
        {
            var configuration = new VelopackUpdateConfiguration("https://github.com/mchave3/CCEM");
            if (!Enum.TryParse(Settings.UpdateChannel, ignoreCase: true, out VelopackChannel channel))
            {
                channel = VelopackChannel.Stable;
                Settings.UpdateChannel = channel.ToString();
            }

            return new VelopackUpdateService(configuration, initialChannel: channel);
        });

        services.AddTransient<MainViewModel>();
        services.AddSingleton<ContextMenuService>();
        services.AddTransient<GeneralSettingViewModel>();
        services.AddTransient<AppUpdateSettingViewModel>();
        services.AddTransient<AboutUsSettingViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Closed += (_, _) => CloseAndFlush();

        MainWindow.Title = MainWindow.AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;
        MainWindow.AppWindow.SetIcon("Assets/AppIcon.ico");

        ThemeService.Initialize(MainWindow);

        MainWindow.Activate();

        _ = InitializeAppAsync();
    }

    private async Task InitializeAppAsync()
    {
        var menuService = GetService<ContextMenuService>();
        if (menuService != null && RuntimeHelper.IsPackaged())
        {
            ContextMenuItem menu = new ContextMenuItem
            {
                Title = "Open CCEM Here",
                Param = @"""{path}""",
                AcceptFileFlag = (int)FileMatchFlagEnum.All,
                AcceptDirectoryFlag = (int)(DirectoryMatchFlagEnum.Directory | DirectoryMatchFlagEnum.Background | DirectoryMatchFlagEnum.Desktop),
                AcceptMultipleFilesFlag = (int)FilesMatchFlagEnum.Each,
                Index = 0,
                Enabled = true,
                Icon = ProcessInfoHelper.GetFileVersionInfo().FileName,
                Exe = "CCEM.exe"
            };

            await menuService.SaveAsync(menu);
        }

        var updateService = GetService<IVelopackUpdateService>();

        try
        {
            var updateResult = await updateService.CheckForUpdatesAsync();

            if (!updateResult.IsUpdateAvailable)
            {
                return;
            }

            var shouldInstall = await PromptForUpdateAsync(updateResult.AvailableVersion);

            if (!shouldInstall)
            {
                return;
            }

            await updateService.DownloadUpdatesAsync(updateResult);
            updateService.ApplyUpdatesAndRestart(updateResult);
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Failed to process application updates.");
        }
    }

    private static Task<bool> PromptForUpdateAsync(string? availableVersion)
    {
        if (MainWindow?.DispatcherQueue is null)
        {
            return Task.FromResult(false);
        }

        var completion = new TaskCompletionSource<bool>();

        if (!MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Update Available",
                    Content = $"A new version ({availableVersion ?? "unknown"}) is available. Would you like to download and install it now?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "No"
                };

                if (MainWindow.Content is FrameworkElement root)
                {
                    dialog.XamlRoot = root.XamlRoot;
                }

                var dialogResult = await dialog.ShowAsync();
                completion.TrySetResult(dialogResult == ContentDialogResult.Primary);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Failed to display update dialog.");
                completion.TrySetResult(false);
            }
        }))
        {
            completion.TrySetResult(false);
        }

        return completion.Task;
    }

    private static void ConfigureApplicationLogger()
    {
        ConfigureLogger(new LoggerConfigurationOptions
        {
            Version = ProcessInfoHelper.Version,
            LogDirectoryPath = Constants.LogDirectoryPath,
            LogFilePath = Constants.LogFilePath,
            MinimumLevel = Settings.UseDeveloperMode ? LogLevel.Debug : LogLevel.Information,
            EnableDebugSink = true
        });
    }
}
