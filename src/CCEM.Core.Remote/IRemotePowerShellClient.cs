using System.Threading;
using System.Threading.Tasks;

namespace CCEM.Core.Remote;

/// <summary>
/// Executes PowerShell locally or via WinRM.
/// </summary>
public interface IRemotePowerShellClient
{
    /// <summary>
    /// Invoke a PowerShell script/command.
    /// </summary>
    /// <param name="request">Invocation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PowerShellInvocationResult> InvokeAsync(PowerShellInvocationRequest request, CancellationToken cancellationToken = default);
}
