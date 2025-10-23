using System;
using CCEM.Core.Velopack.Models;
using CCEM.Core.Velopack.Services;

namespace CCEM.ViewModels;

public partial class GeneralSettingViewModel : ObservableObject
{
    private readonly IVelopackUpdateService _updateService;
    private bool _isNightlyChannelSelected;

    public GeneralSettingViewModel(IVelopackUpdateService updateService)
    {
        _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));

        _isNightlyChannelSelected = GetChannelFromSettings() == VelopackChannel.Nightly;
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
            }
        }
    }

    public string CurrentChannelName => _isNightlyChannelSelected ? "Nightly" : "Stable";
    public string ChannelDisplayText => $"Current channel: {CurrentChannelName}";

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
