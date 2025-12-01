using CCEM.Core.Sccm.Contracts;
using CCEM.Core.Sccm.Models;

namespace CCEM.Core.Sccm.Services;

/// <summary>
/// Placeholder SCCM client service; to be extended with real WMI/PS queries.
/// </summary>
public sealed class SccmClientService : ISccmClientService
{
    public Task<SccmClientSummary> GetClientSummaryAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();

        var summary = new SccmClientSummary(
            ClientVersion: null,
            CachePath: null,
            SiteCode: connection.SiteCode,
            LastHardwareInventory: null,
            LastSoftwareInventory: null,
            LastPolicyRequest: null);

        return Task.FromResult(summary);
    }

    public Task<IReadOnlyList<SccmComponentStatus>> GetComponentsAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult((IReadOnlyList<SccmComponentStatus>)Array.Empty<SccmComponentStatus>());
    }

    public Task<IReadOnlyList<SccmServiceStatus>> GetServicesAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult((IReadOnlyList<SccmServiceStatus>)Array.Empty<SccmServiceStatus>());
    }

    public Task<IReadOnlyList<SccmProcessInfo>> GetProcessesAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult((IReadOnlyList<SccmProcessInfo>)Array.Empty<SccmProcessInfo>());
    }

    public Task<SccmOperationStatus> TriggerEvaluationAsync(SccmConnectionInfo connection, string evaluationName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        if (string.IsNullOrWhiteSpace(evaluationName))
        {
            throw new ArgumentException("Evaluation name is required.", nameof(evaluationName));
        }

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new SccmOperationStatus(true, "Not yet implemented"));
    }
}
