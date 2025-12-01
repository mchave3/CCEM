namespace CCEM.Core.Sccm.Models;

/// <summary>
/// Connection info for SCCM/ConfigMgr client endpoints.
/// </summary>
public sealed record SccmConnectionInfo(
    string HostName,
    int Port = 5985,
    bool UseSsl = false,
    string? SiteCode = null,
    TimeSpan? OperationTimeout = null);
