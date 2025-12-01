using CCEM.Core.Sccm.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CCEM.ViewModels.Sccm;

/// <summary>
/// Base view model for SCCM-related pages with common connection settings.
/// </summary>
public abstract partial class SccmViewModelBase : ObservableObject
{
    [ObservableProperty]
    private string _targetHost = "localhost";

    [ObservableProperty]
    private int _port = 5985;

    [ObservableProperty]
    private bool _useSsl;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _error;

    protected SccmConnectionInfo CreateConnection() =>
        new(TargetHost, Port, UseSsl);
}
