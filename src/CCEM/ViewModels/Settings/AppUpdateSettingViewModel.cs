using CCEM.Core.Velopack.Models;
using CCEM.Core.Velopack.Services;
using Windows.System;

namespace CCEM.ViewModels;

public partial class AppUpdateSettingViewModel : ObservableObject
{
    private const string LatestReleaseUrl = "https://github.com/mchave3/CCEM/releases/latest";

    private readonly IVelopackUpdateService _updateService;
    private readonly bool _updatesSupported;
    private VelopackUpdateCheckResult? _lastCheckResult;
    private string _changeLog = string.Empty;

    [ObservableProperty]
    private string currentVersion;

    [ObservableProperty]
    private string lastUpdateCheck;

    [ObservableProperty]
    private bool isUpdateAvailable;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isCheckButtonEnabled = true;

    [ObservableProperty]
    private string loadingStatus = "Status";

    public AppUpdateSettingViewModel(IVelopackUpdateService updateService)
    {
        _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));

        CurrentVersion = $"Current Version {ProcessInfoHelper.VersionWithPrefix}";
        LastUpdateCheck = Settings.LastUpdateCheck;

        _updatesSupported = _updateService.IsRunningInstalledVersion();
        if (_updatesSupported)
        {
            LoadingStatus = "Press the button to check for updates.";
        }
        else
        {
            LoadingStatus = "Updates are only available when CCEM is installed.";
            IsCheckButtonEnabled = false;
        }
    }

    [RelayCommand]
    private async Task CheckForUpdateAsync()
    {
        if (!_updatesSupported)
        {
            return;
        }

        IsLoading = true;
        IsUpdateAvailable = false;
        IsCheckButtonEnabled = false;
        LoadingStatus = "Checking for new version...";

        try
        {
            LastUpdateCheck = DateTime.Now.ToShortDateString();
            Settings.LastUpdateCheck = LastUpdateCheck;

            _lastCheckResult = await _updateService.CheckForUpdatesAsync();

            if (_lastCheckResult.IsUpdateAvailable)
            {
                IsUpdateAvailable = true;
                _changeLog = _lastCheckResult.ReleaseNotesMarkdown ?? "Release notes are not available.";
                LoadingStatus = $"Version {_lastCheckResult.AvailableVersion} is ready to install.";
            }
            else
            {
                _changeLog = string.Empty;
                LoadingStatus = "You are using the latest version.";
            }
        }
        catch (Exception ex)
        {
            LoadingStatus = ex.Message;
        }
        finally
        {
            IsLoading = false;
            IsCheckButtonEnabled = true;
        }
    }

    [RelayCommand]
    private async Task GoToUpdateAsync()
    {
        if (!_updatesSupported)
        {
            await Launcher.LaunchUriAsync(new Uri(LatestReleaseUrl));
            return;
        }

        if (_lastCheckResult is null || !_lastCheckResult.IsUpdateAvailable)
        {
            await Launcher.LaunchUriAsync(new Uri(LatestReleaseUrl));
            return;
        }

        IsLoading = true;
        IsCheckButtonEnabled = false;
        LoadingStatus = "Downloading update...";

        try
        {
            await _updateService.DownloadUpdatesAsync(_lastCheckResult);
            LoadingStatus = "Restarting to finish the update...";
            _updateService.ApplyUpdatesAndRestart(_lastCheckResult);
        }
        catch (Exception ex)
        {
            LoadingStatus = ex.Message;
        }
        finally
        {
            IsLoading = false;
            IsCheckButtonEnabled = true;
        }
    }

    [RelayCommand]
    private async Task GetReleaseNotesAsync()
    {
        if (!_updatesSupported || string.IsNullOrWhiteSpace(_changeLog))
        {
            await Launcher.LaunchUriAsync(new Uri(LatestReleaseUrl));
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Release Notes",
            CloseButtonText = "Close",
            Content = new ScrollViewer
            {
                Content = new TextBlock
                {
                    Text = _changeLog,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10)
                },
                Margin = new Thickness(10)
            },
            Margin = new Thickness(10),
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = App.MainWindow.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }
}
