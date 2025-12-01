namespace CCEM.Core.Sccm.Models;

public sealed record WmiQuery(string Scope, string Query, TimeSpan? Timeout = null);
