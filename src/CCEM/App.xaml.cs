using System;
using System.IO;
using System.Threading.Tasks;
using CCEM.Core.Velopack.Models;
using CCEM.Core.Velopack.Services;
using CCEM.Services;
using Velopack;
using Velopack.Locators;

namespace CCEM;

public partial class App : Application
{
    /// <summary>
    /// Gets the singleton application object.
    /// </summary>
    public new static App Current => (App)Application.Current;

    /// <summary>
    /// Gets the main window of the application.
    /// </summary>
    public static Window MainWindow = Window.Current;

    /// <summary>
    /// Gets the window handle (HWND) of the main window.
    /// </summary>
    public static IntPtr Hwnd => WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Gets the JSON navigation service.
    /// </summary>
    public IJsonNavigationService NavService => GetService<IJsonNavigationService>();

    /// <summary>
    /// Gets the theme service.
    /// </summary>
    public IThemeService ThemeService => GetService<IThemeService>();

    /// <summary>
    /// The minimum duration to show the splash screen.
    /// </summary>
    private static readonly TimeSpan MinimumSplashDuration = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Gets a service of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static T GetService<T>() where T : class
    {
        if ((App.Current as App)!.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    #region Application Initialization
    /// <summary>
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        // Initialize Velopack application.
        VelopackApp.Build().Run();

        // Configure logger.
        ConfigureApplicationLogger();

        // Global exception handling.
        UnhandledException += (s, e) => Logger?.Error(e.Exception, "UnhandledException");

        // Configure services.
        Services = ConfigureServices();

        // Initialize XAML components.
        this.InitializeComponent();
    }
    #endregion Application Initialization

    #region Service Configuration
    /// <summary>
    /// Configures the dependency injection services.
    /// </summary>
    /// <returns></returns>
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IJsonNavigationService, JsonNavigationService>();
        services.AddSingleton<IUpdateDialogService, UpdateDialogService>();
        services.AddSingleton<ISccmConnectionService, SccmConnectionService>();
        services.AddSingleton<IVelopackUpdateService>(_ =>
        {
            // Configure Velopack update service.
            var configuration = new VelopackUpdateConfiguration("https://github.com/mchave3/CCEM");
            var channel = ResolveInitialUpdateChannel();

            return new VelopackUpdateService(configuration, initialChannel: channel);
        });

        services.AddTransient<MainViewModel>();
        services.AddTransient<SccmConnectionViewModel>();
        services.AddTransient<SccmServicesViewModel>();
        services.AddTransient<SccmProcessesViewModel>();
        services.AddTransient<SccmCacheViewModel>();
        services.AddTransient<SccmSoftwareUpdatesViewModel>();
        services.AddTransient<SccmLogsViewModel>();
        services.AddTransient<SccmWmiBrowserViewModel>();
        services.AddTransient<SccmAgentActionsViewModel>();
        services.AddTransient<SccmInstalledSoftwareViewModel>();
        services.AddTransient<SccmComponentsViewModel>();
        services.AddTransient<SccmAdvertisementsViewModel>();
        services.AddTransient<SccmSoftwareDistributionAppsViewModel>();
        services.AddTransient<SccmSoftwareDistributionSummaryViewModel>();
        services.AddTransient<SccmAllUpdatesViewModel>();
        services.AddTransient<SccmExecutionHistoryViewModel>();
        services.AddTransient<SccmServiceWindowsViewModel>();
        services.AddTransient<SccmCollectionVariablesViewModel>();
        services.AddTransient<SccmCcmEvalViewModel>();
        services.AddTransient<SccmPowerSettingsViewModel>();
        services.AddTransient<SccmAgentSettingsViewModel>();
        services.AddTransient<SccmSettingsMgmtViewModel>();
        services.AddTransient<SccmEventMonitoringViewModel>();
        services.AddTransient<SccmInstallRepairViewModel>();
        services.AddTransient<SccmToolsViewModel>();
        services.AddSingleton<ContextMenuService>();
        services.AddTransient<GeneralSettingViewModel>();
        services.AddTransient<AppUpdateSettingViewModel>();
        services.AddTransient<AboutUsSettingViewModel>();

        return services.BuildServiceProvider();
    }
    #endregion Service Configuration

    #region OnLaunched
    /// <summary>
    /// Invoked when the application is launched normally by the end user. Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args"></param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Create the main window.
        var window = new MainWindow();
        MainWindow = window;
        window.Closed += (_, _) => CloseAndFlush();

        // Set window title and icon.
        window.Title = window.AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;
        window.AppWindow.SetIcon("Assets/AppIcon.ico");

        // Initialize theme service.
        ThemeService.Initialize(window);

        // Start loading components in the background.
        _ = LoadComponentsAsync(window);

        // Activate the window.
        window.Activate();
    }
    #endregion OnLaunched

    #region Application Component Loaders
    /// <summary>
    /// Background component loader.
    /// </summary>
    private async Task LoadComponentsAsync(MainWindow window)
    {
        try
        {
            // Start tasks to load components in parallel.
            var entryAnimationTask = window.DoEntryAnimationAsync();
            var contextMenuTask = EnsureContextMenuAsync();
            var updateTask = CheckForUpdatesAsync();
            var minimumDurationTask = Task.Delay(MinimumSplashDuration);

            // Wait for all tasks to complete.
            await Task.WhenAll(entryAnimationTask, contextMenuTask, updateTask, minimumDurationTask);
            Logger?.Information("LoadComponentsAsync finished. Showing module selection.");

            // Switch to the module selection interface.
            window.ShowModuleSelection();
        }
        catch (Exception ex)
        {
            Logger?.Fatal(ex, "Critical failure while loading components.");
            throw;
        }
    }

    /// <summary>
    /// Ensures that the Windows context menu integration is configured.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Checks for application updates and prompts the user to install them if available.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Configures the application logger.
    /// </summary>
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
    #endregion Application Component Loaders

    /// <summary>
    /// Resolves the initial update channel based on packaged channel and existing settings.
    /// </summary>
    /// <returns></returns>
    private static VelopackChannel ResolveInitialUpdateChannel()
    {
        // Get the packaged channel from Velopack locator.
        var packagedChannelName = VelopackLocator.Current?.Channel;
        VelopackChannel? packagedChannel = null;

        // Parse the packaged channel if available.
        if (!string.IsNullOrWhiteSpace(packagedChannelName) &&
            Enum.TryParse(packagedChannelName, ignoreCase: true, out VelopackChannel parsedPackagedChannel))
        {
            packagedChannel = parsedPackagedChannel;
        }

        var hasExistingSettings = File.Exists(Constants.AppConfigPath);

        // Determine the initial update channel based on existing settings and packaged channel.
        if (hasExistingSettings &&
            Enum.TryParse(Settings.UpdateChannel, ignoreCase: true, out VelopackChannel persistedChannel))
        {
            if (packagedChannel.HasValue)
            {
                var packagedChannelString = packagedChannel.Value.ToString();
                var lastInstalledChannel = Settings.LastInstalledChannel;
                var installerChannelChanged = !string.Equals(
                    lastInstalledChannel,
                    packagedChannelString,
                    StringComparison.OrdinalIgnoreCase);

                if (installerChannelChanged || string.IsNullOrWhiteSpace(lastInstalledChannel))
                {
                    Settings.LastInstalledChannel = packagedChannelString;
                }

                if (installerChannelChanged && !Settings.IsUpdateChannelOverridden)
                {
                    Settings.UpdateChannel = packagedChannelString;
                    Settings.IsUpdateChannelOverridden = false;
                    return packagedChannel.Value;
                }
            }

            return persistedChannel;
        }

        // No existing settings; use packaged channel or fallback to Stable.
        if (packagedChannel.HasValue)
        {
            var packagedChannelString = packagedChannel.Value.ToString();
            Settings.LastInstalledChannel = packagedChannelString;
            Settings.IsUpdateChannelOverridden = false;
            Settings.UpdateChannel = packagedChannelString;
            return packagedChannel.Value;
        }

        var fallbackChannel = VelopackChannel.Stable;
        var fallbackChannelString = fallbackChannel.ToString();
        Settings.LastInstalledChannel = fallbackChannelString;
        Settings.IsUpdateChannelOverridden = false;
        Settings.UpdateChannel = fallbackChannelString;
        return fallbackChannel;
    }
}
