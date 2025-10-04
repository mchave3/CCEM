namespace CCEM.SCCM.Models;

/// <summary>
/// Model representing an SCCM policy
/// </summary>
public partial class PolicyModel : ObservableObject
{
    [ObservableProperty]
    private string _policyId = string.Empty;

    [ObservableProperty]
    private string _policyName = string.Empty;

    [ObservableProperty]
    private string _policyType = string.Empty;

    [ObservableProperty]
    private string _policySource = string.Empty;

    [ObservableProperty]
    private string _version = string.Empty;

    [ObservableProperty]
    private DateTime? _downloadTime;

    [ObservableProperty]
    private DateTime? _compileTime;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _targetCollection;

    [ObservableProperty]
    private int _priority;
}
