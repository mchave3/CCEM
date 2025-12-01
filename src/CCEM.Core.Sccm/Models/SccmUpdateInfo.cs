namespace CCEM.Core.Sccm.Models;

public sealed record SccmUpdateInfo(
    string? Id,
    string? Name,
    string? ArticleId,
    string? State,
    DateTimeOffset? LastStateChange);
