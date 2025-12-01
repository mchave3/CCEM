namespace CCEM.Core.Sccm.Models;

public sealed record SccmOperationStatus(bool Success, string? Message = null);
