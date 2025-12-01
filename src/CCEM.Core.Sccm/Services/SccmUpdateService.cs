using CCEM.Core.Sccm.Contracts;
using CCEM.Core.Sccm.Models;

namespace CCEM.Core.Sccm.Services;

/// <summary>
/// Placeholder software update service.
/// </summary>
public sealed class SccmUpdateService : ISccmUpdateService
{
    public Task<IReadOnlyList<SccmUpdateInfo>> GetUpdatesAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<SccmUpdateInfo> sample =
        [
            new("KB503200", "2025-01 Cumulative Update", "503200", "Installed", DateTimeOffset.Now.AddDays(-10)),
            new("KB503210", "2025-01 .NET Rollup", "503210", "Pending", DateTimeOffset.Now.AddDays(-2)),
            new("KB503215", "Defender Platform Update", "503215", "Downloading", DateTimeOffset.Now.AddHours(-4))
        ];
        return Task.FromResult(sample);
    }
}
