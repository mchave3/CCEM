namespace CCEM.SCCM.Models;

/// <summary>
/// Model representing an SCCM application
/// </summary>
public partial class ApplicationModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _version = string.Empty;

    [ObservableProperty]
    private string _publisher = string.Empty;

    [ObservableProperty]
    private string _installState = string.Empty;

    [ObservableProperty]
    private DateTime? _installDate;

    [ObservableProperty]
    private string? _packageId;

    [ObservableProperty]
    private string? _programId;

    [ObservableProperty]
    private bool _isRequired;

    [ObservableProperty]
    private DateTime? _deadline;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private long _estimatedSize;

    [ObservableProperty]
    private int _percentComplete;
}
