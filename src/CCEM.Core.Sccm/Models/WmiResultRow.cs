namespace CCEM.Core.Sccm.Models;

public sealed record WmiResultRow(IReadOnlyDictionary<string, object?> Properties);
