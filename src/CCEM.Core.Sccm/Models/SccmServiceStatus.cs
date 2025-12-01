namespace CCEM.Core.Sccm.Models;

public sealed record SccmServiceStatus(
    string Name,
    string? Status,
    bool StartupAutomatic,
    string? DisplayName);
