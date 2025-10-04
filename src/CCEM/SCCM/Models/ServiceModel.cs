namespace CCEM.SCCM.Models;

/// <summary>
/// Model representing an SCCM service
/// </summary>
public partial class ServiceModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private string _startMode = string.Empty;

    [ObservableProperty]
    private string _state = string.Empty;

    [ObservableProperty]
    private int _processId;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _pathName;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private DateTime? _lastStartTime;
}
