namespace CCEM.SCCM.Models;

/// <summary>
/// Model representing a software update
/// </summary>
public partial class UpdateModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _articleId = string.Empty;

    [ObservableProperty]
    private string _bulletinId = string.Empty;

    [ObservableProperty]
    private string _updateId = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private string _severity = string.Empty;

    [ObservableProperty]
    private DateTime? _releaseDate;

    [ObservableProperty]
    private DateTime? _deadline;

    [ObservableProperty]
    private bool _isRequired;

    [ObservableProperty]
    private bool _isInstalled;

    [ObservableProperty]
    private int _percentComplete;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private long _downloadSize;

    [ObservableProperty]
    private string? _errorCode;
}
