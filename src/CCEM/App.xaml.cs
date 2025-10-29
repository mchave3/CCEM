using System;
using System.Threading.Tasks;
using CCEM.Core.Velopack.Models;
using CCEM.Core.Velopack.Services;
using CCEM.Services;
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

    private static readonly TimeSpan MinimumSplashDuration = TimeSpan.FromSeconds(3);

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
        services.AddSingleton<IUpdateDialogService, UpdateDialogService>();
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
        var window = new MainWindow();
        MainWindow = window;
        window.Closed += (_, _) => CloseAndFlush();

        window.Title = window.AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;
        window.AppWindow.SetIcon("Assets/AppIcon.ico");

        ThemeService.Initialize(window);

        // Start loading components in background (UniGetUI-style)
        _ = LoadComponentsAsync(window);

        window.Activate();
    }

    /// <summary>
    /// Background component loader similar to UniGetUI
    /// </summary>
    private async Task LoadComponentsAsync(MainWindow window)
    {
        try
        {
            // Start the entry animation/loading display
            var entryAnimationTask = window.DoEntryAnimationAsync();

            // Initialize supporting services in parallel
            var contextMenuTask = EnsureContextMenuAsync();

            var updateTask = CheckForUpdatesAsync();
            var minimumDurationTask = Task.Delay(MinimumSplashDuration);

            await Task.WhenAll(entryAnimationTask, contextMenuTask, updateTask, minimumDurationTask);

            Logger?.Information("LoadComponentsAsync finished. Switching to interface.");
            window.SwitchToInterface();
        }
        catch (Exception ex)
        {
            Logger?.Fatal(ex, "Critical failure while loading components.");
            throw;
        }
    }

    private static async Task EnsureContextMenuAsync()
    {
        try
        {
            var menuService = GetService<ContextMenuService>();
            if (menuService is null)
            {
                Logger?.Debug("Context menu service unavailable; skipping integration.");
                return;
            }

            if (!RuntimeHelper.IsPackaged())
            {
                Logger?.Debug("Application is not packaged; skipping context menu registration.");
                return;
            }

            Logger?.Information("Configuring Windows context menu integration...");

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

            await menuService.SaveAsync(menu).ConfigureAwait(false);
            Logger?.Information("Context menu integration ensured.");
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Failed to configure context menu.");
        }
    }

    private static async Task CheckForUpdatesAsync()
    {
        try
        {
            var updateService = GetService<IVelopackUpdateService>();
            var updateDialogService = GetService<IUpdateDialogService>();

            if (updateService is null || updateDialogService is null)
            {
                Logger?.Warning("Update services unavailable; skipping update workflow.");
                return;
            }

            Logger?.Information("Checking for updates...");

            var updateResult = await updateService.CheckForUpdatesAsync().ConfigureAwait(false);

            if (!updateResult.IsUpdateAvailable)
            {
                Logger?.Information("No application updates available.");
                return;
            }

            Logger?.Information($"Update available: {updateResult.AvailableVersion}");

            var downloaded = await updateDialogService.ShowUpdateAvailableDialogAsync(
                updateResult.AvailableVersion,
                progress => updateService.DownloadUpdatesAsync(updateResult, progress)).ConfigureAwait(false);

            if (!downloaded)
            {
                Logger?.Information("User declined to download updates.");
                return;
            }

            updateService.ApplyUpdatesAndRestart(updateResult);
        }
        catch (Velopack.Exceptions.NotInstalledException)
        {
            Logger?.Debug("Application is running in development mode; skipping update checks.");
        }
        catch (Exception ex)
        {
            Logger?.Error(ex, "Failed to process application updates.");
        }
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
