namespace CCEM.Core.Sccm.Models;

public sealed record SccmClientSummary(
    string? ClientVersion,
    string? CachePath,
    string? SiteCode,
    DateTimeOffset? LastHardwareInventory,
    DateTimeOffset? LastSoftwareInventory,
    DateTimeOffset? LastPolicyRequest);
