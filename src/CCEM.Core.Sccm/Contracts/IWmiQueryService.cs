using CCEM.Core.Sccm.Models;

namespace CCEM.Core.Sccm.Contracts;

public interface IWmiQueryService
{
    Task<IReadOnlyList<WmiResultRow>> QueryAsync(WmiQuery query, CancellationToken cancellationToken = default);
}
