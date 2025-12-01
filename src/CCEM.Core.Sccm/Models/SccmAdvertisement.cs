namespace CCEM.Core.Sccm.Models;

public sealed record SccmAdvertisement(
    string Id,
    string? Name,
    string? Program,
    string? State,
    DateTimeOffset? LastRunTime);
