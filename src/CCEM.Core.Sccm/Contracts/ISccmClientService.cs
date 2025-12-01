using CCEM.Core.Sccm.Models;

namespace CCEM.Core.Sccm.Contracts;

public interface ISccmClientService
{
    Task<SccmClientSummary> GetClientSummaryAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SccmComponentStatus>> GetComponentsAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SccmServiceStatus>> GetServicesAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SccmProcessInfo>> GetProcessesAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default);
    Task<SccmOperationStatus> TriggerEvaluationAsync(SccmConnectionInfo connection, string evaluationName, CancellationToken cancellationToken = default);
}
