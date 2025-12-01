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
        return Task.FromResult((IReadOnlyList<SccmUpdateInfo>)Array.Empty<SccmUpdateInfo>());
    }
}
