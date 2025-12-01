namespace CCEM.Core.Sccm.Models;

public sealed record SccmServiceWindow(
    string Name,
    DateTimeOffset? Start,
    DateTimeOffset? End,
    string? Recurrence,
    string? Comment);
