namespace CCEM.Core.Sccm.Models;

public sealed record SccmEvaluationStatus(
    string Name,
    DateTimeOffset? LastRun,
    string? Result);
