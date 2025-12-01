using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace CCEM.Core.Remote;

/// <summary>
/// PowerShell client using local runspace or WinRM/WSMan.
/// </summary>
public sealed class WinRmPowerShellClient : IRemotePowerShellClient
{
    private const string DefaultShellUri = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";

    public async Task<PowerShellInvocationResult> InvokeAsync(
        PowerShellInvocationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Script))
        {
            throw new ArgumentException("Script cannot be null or empty.", nameof(request));
        }

        using var runspace = CreateRunspace(request);
        runspace.Open();

        using var ps = PowerShell.Create();
        ps.Runspace = runspace;
        ps.AddScript(request.Script);

        if (request.Parameters is not null)
        {
            foreach (var pair in request.Parameters)
            {
                ps.AddParameter(pair.Key, pair.Value);
            }
        }

        var execTask = Task.Run(() => ps.Invoke(), CancellationToken.None);

        var timedOut = false;
        using var timeoutCts = CreateTimeoutCts(request.OperationTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts?.Token ?? CancellationToken.None);
        using var registration = linkedCts.Token.Register(ps.Stop, useSynchronizationContext: false);

        Collection<PSObject> results;
        try
        {
            results = timeoutCts is null
                ? await execTask.ConfigureAwait(false)
                : await execTask.WaitAsync(linkedCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeoutCts is not null && timeoutCts.IsCancellationRequested)
        {
            timedOut = true;
            return BuildResult(ps, Array.Empty<PSObject>(), timedOut);
        }

        timedOut = timeoutCts is not null && timeoutCts.IsCancellationRequested;
        return BuildResult(ps, results, timedOut);
    }

    private static PowerShellInvocationResult BuildResult(PowerShell ps, IReadOnlyCollection<PSObject> objects, bool timedOut)
    {
        var output = objects.Select(o => o?.BaseObject?.ToString() ?? string.Empty).ToList();
        var errors = ps.Streams.Error.Select(e => e.ToString()).ToList();

        return new PowerShellInvocationResult(output, errors, ps.HadErrors, timedOut);
    }

    private static CancellationTokenSource? CreateTimeoutCts(TimeSpan? timeout)
    {
        if (timeout is null || timeout <= TimeSpan.Zero)
        {
            return null;
        }

        return new CancellationTokenSource(timeout.Value);
    }

    private static Runspace CreateRunspace(PowerShellInvocationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ComputerName))
        {
            return RunspaceFactory.CreateRunspace();
        }

        var port = request.Port ?? (request.UseSsl ? 5986 : 5985);
        var scheme = request.UseSsl ? "https" : "http";
        var uri = new UriBuilder(scheme, request.ComputerName, port, "/wsman").Uri;
        var connectionInfo = new WSManConnectionInfo(
            uri,
            request.ShellUri ?? DefaultShellUri,
            request.Credential)
        {
            OperationTimeout = (int)(request.OperationTimeout?.TotalMilliseconds ?? 120_000),
            OpenTimeout = (int)(request.OperationTimeout?.TotalMilliseconds ?? 30_000)
        };

        return RunspaceFactory.CreateRunspace(connectionInfo);
    }
}
