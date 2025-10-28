using System;
using CCEM.Core.Velopack.Models;
using CCEM.Core.Velopack.Services;
using Microsoft.UI.Xaml;

namespace CCEM.ViewModels;

public partial class GeneralSettingViewModel : ObservableObject
{
    private readonly IVelopackUpdateService _updateService;
    private readonly bool _initialNightlySelection;
    private bool _isNightlyChannelSelected;

    public GeneralSettingViewModel(IVelopackUpdateService updateService)
    {
        _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));

        _isNightlyChannelSelected = GetChannelFromSettings() == VelopackChannel.Nightly;
        _initialNightlySelection = _isNightlyChannelSelected;
        _updateService.SetChannel(_isNightlyChannelSelected ? VelopackChannel.Nightly : VelopackChannel.Stable);
    }

    public bool IsNightlyChannelSelected
    {
        get => _isNightlyChannelSelected;
        set
        {
            if (SetProperty(ref _isNightlyChannelSelected, value))
            {
                var channel = value ? VelopackChannel.Nightly : VelopackChannel.Stable;
                Settings.UpdateChannel = channel.ToString();
                _updateService.SetChannel(channel);

                OnPropertyChanged(nameof(CurrentChannelName));
                OnPropertyChanged(nameof(ChannelDisplayText));
                OnPropertyChanged(nameof(IsRestartRequired));
                OnPropertyChanged(nameof(RestartWarningVisibility));
            }
        }
    }

    public string CurrentChannelName => _isNightlyChannelSelected ? "Nightly" : "Stable";
    public string ChannelDisplayText => $"Current channel: {CurrentChannelName}";
    public bool IsRestartRequired => _isNightlyChannelSelected != _initialNightlySelection;
    public Visibility RestartWarningVisibility => IsRestartRequired ? Visibility.Visible : Visibility.Collapsed;

    private static VelopackChannel GetChannelFromSettings()
    {
        if (Enum.TryParse(Settings.UpdateChannel, ignoreCase: true, out VelopackChannel channel))
        {
            return channel;
        }

        Settings.UpdateChannel = VelopackChannel.Stable.ToString();
        return VelopackChannel.Stable;
    }
}
