namespace CCEM.SCCM.Models;

/// <summary>
/// Model representing an SCCM client component
/// </summary>
public partial class ComponentModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _state = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private DateTime? _lastModified;

    [ObservableProperty]
    private string? _version;

    [ObservableProperty]
    private bool _isEnabled;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _errorCode;
}
