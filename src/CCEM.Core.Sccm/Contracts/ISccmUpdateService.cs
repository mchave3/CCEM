using CCEM.Core.Sccm.Models;

namespace CCEM.Core.Sccm.Contracts;

public interface ISccmUpdateService
{
    Task<IReadOnlyList<SccmUpdateInfo>> GetUpdatesAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default);
}
