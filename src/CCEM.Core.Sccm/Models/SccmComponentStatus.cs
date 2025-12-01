namespace CCEM.Core.Sccm.Models;

public sealed record SccmComponentStatus(
    string Name,
    string? State,
    DateTimeOffset? LastHeartbeat,
    string? LastMessage);
