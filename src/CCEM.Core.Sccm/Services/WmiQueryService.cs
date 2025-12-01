using System.Collections.Generic;
using System.Management;
using CCEM.Core.Sccm.Contracts;
using CCEM.Core.Sccm.Models;

namespace CCEM.Core.Sccm.Services;

/// <summary>
/// Lightweight WMI query wrapper with cancellation and timeout support.
/// </summary>
public sealed class WmiQueryService : IWmiQueryService
{
    public async Task<IReadOnlyList<WmiResultRow>> QueryAsync(WmiQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrWhiteSpace(query.Scope))
        {
            throw new ArgumentException("Scope cannot be null or empty.", nameof(query));
        }

        if (string.IsNullOrWhiteSpace(query.Query))
        {
            throw new ArgumentException("Query cannot be null or empty.", nameof(query));
        }

        cancellationToken.ThrowIfCancellationRequested();

        return await Task.Run(() => Execute(query, cancellationToken), cancellationToken).ConfigureAwait(false);
    }

    private static IReadOnlyList<WmiResultRow> Execute(WmiQuery query, CancellationToken cancellationToken)
    {
        var scope = new ManagementScope(query.Scope);
        if (query.Timeout is { } timeout && timeout > TimeSpan.Zero)
        {
            scope.Options.Timeout = timeout;
        }

        scope.Connect();

        using var searcher = new ManagementObjectSearcher(scope, new ObjectQuery(query.Query))
        {
            Options = { Timeout = query.Timeout ?? TimeSpan.FromSeconds(30) }
        };

        using var results = searcher.Get();
        var rows = new List<WmiResultRow>();

        foreach (ManagementObject obj in results)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var properties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (PropertyData prop in obj.Properties)
            {
                properties[prop.Name] = prop.Value;
            }

            rows.Add(new WmiResultRow(properties));
        }

        return rows;
    }
}
