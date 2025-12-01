using CCEM.Core.Sccm.Models;

namespace CCEM.Core.Sccm.Contracts;

public interface ISccmSoftwareDistributionService
{
    Task<IReadOnlyList<SccmAdvertisement>> GetAdvertisementsAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SccmAdvertisement>> GetExecutionHistoryAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default);
}
