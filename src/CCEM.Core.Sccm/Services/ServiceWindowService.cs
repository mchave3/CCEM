using CCEM.Core.Sccm.Contracts;
using CCEM.Core.Sccm.Models;

namespace CCEM.Core.Sccm.Services;

/// <summary>
/// Placeholder service window operations.
/// </summary>
public sealed class ServiceWindowService : IServiceWindowService
{
    public Task<IReadOnlyList<SccmServiceWindow>> GetServiceWindowsAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult((IReadOnlyList<SccmServiceWindow>)Array.Empty<SccmServiceWindow>());
    }
}
