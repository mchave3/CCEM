using System.Threading;
using System.Threading.Tasks;
using CCEM.Core.Startup;
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

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = new MainWindow();
        MainWindow = window;
        window.Closed += (_, _) => CloseAndFlush();

        window.Title = window.AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;
        window.AppWindow.SetIcon("Assets/AppIcon.ico");

        ThemeService.Initialize(window);

        window.Activate();

        var startupPipeline = BuildStartupPipeline(window);

        try
        {
            await startupPipeline.RunAsync();
        }
        catch (Exception ex)
        {
            Logger?.Fatal(ex, "Critical failure while executing the startup pipeline.");
            throw;
        }
    }

    private AppStartupPipeline BuildStartupPipeline(MainWindow host)
    {
        var options = new StartupPipelineOptions
        {
            InitialStatusMessage = "Preparing CCEM...",
            CompletionStatusMessage = "Loading workspace..."
        };

        var builder = new StartupPipelineBuilder()
            .AddCriticalStep(
                "Context menu registration",
                "Configuring Windows context menu integration...",
                EnsureContextMenuAsync)
            .AddBackgroundStep(
                "Update discovery",
                RunUpdateFlowAsync);

        return builder.Build(host, Services, options, Logger);
    }

    private static async Task EnsureContextMenuAsync(StartupContext context, CancellationToken cancellationToken)
    {
        var menuService = context.GetService<ContextMenuService>();
        if (menuService is null)
        {
            context.Logger?.Debug("Context menu service unavailable; skipping integration.");
            return;
        }

        if (!RuntimeHelper.IsPackaged())
        {
            context.Logger?.Debug("Application is not packaged; skipping context menu registration.");
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

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
        context.Logger?.Information("Context menu integration ensured.");
    }

    private static async Task RunUpdateFlowAsync(StartupContext context, CancellationToken cancellationToken)
    {
        var updateService = context.GetService<IVelopackUpdateService>();
        var updateDialogService = context.GetService<IUpdateDialogService>();

        if (updateService is null || updateDialogService is null)
        {
            context.Logger?.Warning("Update services unavailable; skipping update workflow.");
            return;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var updateResult = await updateService.CheckForUpdatesAsync().ConfigureAwait(false);

            if (!updateResult.IsUpdateAvailable)
            {
                context.Logger?.Information("No application updates available.");
                return;
            }

            var downloaded = await updateDialogService.ShowUpdateAvailableDialogAsync(
                updateResult.AvailableVersion,
                progress => updateService.DownloadUpdatesAsync(updateResult, progress)).ConfigureAwait(false);

            if (!downloaded)
            {
                context.Logger?.Information("User declined to download updates.");
                return;
            }

            updateService.ApplyUpdatesAndRestart(updateResult);
        }
        catch (OperationCanceledException)
        {
            context.Logger?.Warning("Update workflow cancelled by caller.");
        }
        catch (Exception ex)
        {
            context.Logger?.Error(ex, "Failed to process application updates.");
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
