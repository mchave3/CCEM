namespace CCEM.Core.Sccm.Models;

public sealed record SccmInstalledSoftware(
    string Name,
    string? Version,
    string? Publisher,
    DateTimeOffset? InstallDate);
