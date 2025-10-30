using System;
using CCEM.Core.Velopack.Models;
using CCEM.Core.Velopack.Services;
using Microsoft.UI.Xaml;
using Velopack.Locators;

namespace CCEM.ViewModels;

public partial class GeneralSettingViewModel : ObservableObject
{
    private readonly IVelopackUpdateService _updateService;
    private readonly bool _initialBetaSelection;
    private bool _isBetaChannelSelected;

    public GeneralSettingViewModel(IVelopackUpdateService updateService)
    {
        _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));

        _isBetaChannelSelected = GetChannelFromSettings() == VelopackChannel.Beta;
        _initialBetaSelection = _isBetaChannelSelected;
        _updateService.SetChannel(_isBetaChannelSelected ? VelopackChannel.Beta : VelopackChannel.Stable);
    }

    public bool IsBetaChannelSelected
    {
        get => _isBetaChannelSelected;
        set
        {
            if (SetProperty(ref _isBetaChannelSelected, value))
            {
                var channel = value ? VelopackChannel.Beta : VelopackChannel.Stable;
                var channelName = channel.ToString();
                Settings.UpdateChannel = channelName;

                var packagedChannelName = VelopackLocator.Current?.Channel;
                if (!string.IsNullOrWhiteSpace(packagedChannelName) &&
                    packagedChannelName.Equals(channelName, StringComparison.OrdinalIgnoreCase))
                {
                    Settings.IsUpdateChannelOverridden = false;
                }
                else
                {
                    Settings.IsUpdateChannelOverridden = true;
                }

                _updateService.SetChannel(channel);

                OnPropertyChanged(nameof(CurrentChannelName));
                OnPropertyChanged(nameof(ChannelDisplayText));
                OnPropertyChanged(nameof(IsRestartRequired));
                OnPropertyChanged(nameof(RestartWarningVisibility));
            }
        }
    }

    public string CurrentChannelName => _isBetaChannelSelected ? "Beta" : "Stable";
    public string ChannelDisplayText => $"Current channel: {CurrentChannelName}";
    public bool IsRestartRequired => _isBetaChannelSelected != _initialBetaSelection;
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
