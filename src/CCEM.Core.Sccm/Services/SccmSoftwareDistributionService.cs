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
        IReadOnlyList<SccmAdvertisement> sample =
        [
            new("ADV001", "Deploy Company Portal", "Install", "Success", DateTimeOffset.Now.AddHours(-6)),
            new("ADV002", "Install .NET Runtime", "Install", "InProgress", DateTimeOffset.Now.AddHours(-2))
        ];
        return Task.FromResult(sample);
    }

    public Task<IReadOnlyList<SccmAdvertisement>> GetExecutionHistoryAsync(SccmConnectionInfo connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<SccmAdvertisement> sample =
        [
            new("ADV001", "Deploy Company Portal", "Install", "Success", DateTimeOffset.Now.AddHours(-6)),
            new("ADV003", "Software Metering", "Evaluate", "Failed", DateTimeOffset.Now.AddHours(-1))
        ];
        return Task.FromResult(sample);
    }
}
