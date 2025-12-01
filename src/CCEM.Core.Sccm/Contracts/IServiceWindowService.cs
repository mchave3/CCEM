using CCEM.Core.Sccm.Models;

namespace CCEM.Core.Sccm.Contracts;

public interface IServiceWindowService
{
    Task<IReadOnlyList<SccmServiceWindow>> GetServiceWindowsAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default);
}
