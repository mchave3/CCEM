using CCEM.Core.Sccm.Contracts;
using CCEM.Core.Sccm.Models;

namespace CCEM.Core.Sccm.Services;

/// <summary>
/// Placeholder software distribution service.
/// </summary>
public sealed class SccmSoftwareDistributionService : ISccmSoftwareDistributionService
{
    public Task<IReadOnlyList<SccmAdvertisement>> GetAdvertisementsAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult((IReadOnlyList<SccmAdvertisement>)Array.Empty<SccmAdvertisement>());
    }

    public Task<IReadOnlyList<SccmAdvertisement>> GetExecutionHistoryAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult((IReadOnlyList<SccmAdvertisement>)Array.Empty<SccmAdvertisement>());
    }
}
