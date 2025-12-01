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
        IReadOnlyList<SccmComponentStatus> sample =
        [
            new("CCMExec", "OK", DateTimeOffset.Now.AddMinutes(-5), "Running"),
            new("WUAHandler", "OK", DateTimeOffset.Now.AddMinutes(-15), "Idle"),
            new("InventoryAgent", "Pending", DateTimeOffset.Now.AddHours(-1), "Next cycle scheduled")
        ];
        return Task.FromResult(sample);
    }

    public Task<IReadOnlyList<SccmServiceStatus>> GetServicesAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<SccmServiceStatus> sample =
        [
            new("CcmExec", "Running", true, "SMS Agent Host"),
            new("WinRM", "Running", true, "Windows Remote Management"),
            new("Wuauserv", "Running", true, "Windows Update")
        ];
        return Task.FromResult(sample);
    }

    public Task<IReadOnlyList<SccmProcessInfo>> GetProcessesAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<SccmProcessInfo> sample =
        [
            new("CCMExec", 1234, 1.2, 90_000_000),
            new("WmiPrvSE", 2222, 0.8, 75_000_000),
            new("PowerShell", 3333, 5.1, 120_000_000)
        ];
        return Task.FromResult(sample);
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

    public Task<IReadOnlyList<SccmCacheEntry>> GetCacheAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<SccmCacheEntry> sample =
        [
            new("Content_001", @"C:\Windows\ccmcache\1", 512, true),
            new("Content_002", @"C:\Windows\ccmcache\2", 128, false),
        ];
        return Task.FromResult(sample);
    }

    public Task<IReadOnlyList<SccmInstalledSoftware>> GetInstalledSoftwareAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<SccmInstalledSoftware> sample =
        [
            new("Configuration Manager Client", "5.00.9096.1000", "Microsoft", DateTimeOffset.Now.AddMonths(-3)),
            new("Company Portal", "1.2.3.4", "Contoso", DateTimeOffset.Now.AddMonths(-1))
        ];
        return Task.FromResult(sample);
    }

    public Task<IReadOnlyList<SccmEvaluationStatus>> GetEvaluationsAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<SccmEvaluationStatus> sample =
        [
            new("Hardware Inventory", DateTimeOffset.Now.AddHours(-5), "Success"),
            new("Software Updates Deployment", DateTimeOffset.Now.AddHours(-2), "Success"),
            new("Application Deployment Evaluation", DateTimeOffset.Now.AddHours(-12), "Pending")
        ];
        return Task.FromResult(sample);
    }
}
