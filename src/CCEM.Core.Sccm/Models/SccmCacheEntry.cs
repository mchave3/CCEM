namespace CCEM.Core.Sccm.Models;

public sealed record SccmCacheEntry(
    string ContentId,
    string? Location,
    double? SizeMb,
    bool Persisted);
