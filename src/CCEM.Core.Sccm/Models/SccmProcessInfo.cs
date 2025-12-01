namespace CCEM.Core.Sccm.Models;

public sealed record SccmProcessInfo(
    string Name,
    int Id,
    double? CpuPercent,
    long? WorkingSetBytes);
