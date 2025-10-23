using CCEM.Core.Velopack.Bootstrap;
using CCEM.Core.Velopack.Models;
using CCEM.Core.Velopack.Services;

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
        VelopackBootstrapper.Initialize();
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
            var updateService = new VelopackUpdateService(configuration);
            if (!Enum.TryParse(Settings.UpdateChannel, ignoreCase: true, out VelopackChannel channel))
            {
                channel = VelopackChannel.Stable;
                Settings.UpdateChannel = channel.ToString();
            }

            updateService.SetChannel(channel);
            return updateService;
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

        MainWindow.Title = MainWindow.AppWindow.Title = ProcessInfoHelper.ProductNameAndVersion;
        MainWindow.AppWindow.SetIcon("Assets/AppIcon.ico");

        ThemeService.Initialize(MainWindow);

        MainWindow.Activate();

        InitializeApp();
    }

    private async void InitializeApp()
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

        if (Settings.UseDeveloperMode)
        {
            ConfigureLogger();
        }

        UnhandledException += (s, e) => Logger?.Error(e.Exception, "UnhandledException");
    }
}
