using System;
using System.Threading.Tasks;
using CCEM.Core.Velopack.Models;
using CCEM.Core.Velopack.Services;
using CCEM.Services;
using Microsoft.UI.Xaml;
using Windows.System;

namespace CCEM.ViewModels;

public partial class AppUpdateSettingViewModel : ObservableObject
{
    private const string LatestReleaseUrl = "https://github.com/mchave3/CCEM/releases/latest";

    private readonly IVelopackUpdateService _updateService;
    private readonly IUpdateDialogService _updateDialogService;
    private readonly bool _updatesSupported;
    private VelopackUpdateCheckResult? _lastCheckResult;
    private string _changeLog = string.Empty;

    private string _currentVersion = string.Empty;
    public string CurrentVersion
    {
        get => _currentVersion;
        set => SetProperty(ref _currentVersion, value);
    }

    private string _lastUpdateCheck = string.Empty;
    public string LastUpdateCheck
    {
        get => _lastUpdateCheck;
        set => SetProperty(ref _lastUpdateCheck, value);
    }

    private bool _isUpdateAvailable;
    public bool IsUpdateAvailable
    {
        get => _isUpdateAvailable;
        set => SetProperty(ref _isUpdateAvailable, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private bool _isCheckButtonEnabled = true;
    public bool IsCheckButtonEnabled
    {
        get => _isCheckButtonEnabled;
        set => SetProperty(ref _isCheckButtonEnabled, value);
    }

    private string _loadingStatus = "Status";
    public string LoadingStatus
    {
        get => _loadingStatus;
        set => SetProperty(ref _loadingStatus, value);
    }

    public AppUpdateSettingViewModel(IVelopackUpdateService updateService, IUpdateDialogService updateDialogService)
    {
        _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));
        _updateDialogService = updateDialogService ?? throw new ArgumentNullException(nameof(updateDialogService));

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

                if (await DownloadAndApplyUpdateAsync())
                {
                    return;
                }
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
        LoadingStatus = "Preparing to download update...";

        try
        {
            var downloaded = await DownloadAndApplyUpdateAsync();
            if (!downloaded)
            {
                LoadingStatus = "Update postponed.";
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

    private async Task<bool> DownloadAndApplyUpdateAsync()
    {
        if (_lastCheckResult is null)
        {
            return false;
        }

        var downloaded = await _updateDialogService.ShowUpdateAvailableDialogAsync(
            _lastCheckResult.AvailableVersion,
            progress => _updateService.DownloadUpdatesAsync(_lastCheckResult, progress));

        if (downloaded)
        {
            LoadingStatus = "Restarting to finish the update...";
            _updateService.ApplyUpdatesAndRestart(_lastCheckResult);
        }

        return downloaded;
    }
}
