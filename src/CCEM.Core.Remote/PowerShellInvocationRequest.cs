using System.Management.Automation;

namespace CCEM.Core.Remote;

/// <summary>
/// Describes a PowerShell invocation request (local or remote).
/// </summary>
public sealed class PowerShellInvocationRequest
{
    /// <summary>
    /// PowerShell script or command text to execute.
    /// </summary>
    public required string Script { get; init; }

    /// <summary>
    /// Optional parameters passed to the script.
    /// </summary>
    public IDictionary<string, object?>? Parameters { get; init; }

    /// <summary>
    /// Remote computer name or FQDN. When null/empty, execution is local.
    /// </summary>
    public string? ComputerName { get; init; }

    /// <summary>
    /// Optional port for WinRM/WSMan (defaults to 5985 or 5986 based on UseSsl).
    /// </summary>
    public int? Port { get; init; }

    /// <summary>
    /// Whether to use HTTPS for WinRM.
    /// </summary>
    public bool UseSsl { get; init; }

    /// <summary>
    /// Optional credential for remote authentication.
    /// </summary>
    public PSCredential? Credential { get; init; }

    /// <summary>
    /// Optional operation timeout; when exceeded, the invocation is stopped.
    /// </summary>
    public TimeSpan? OperationTimeout { get; init; }

    /// <summary>
    /// Optional shell URI (defaults to the Microsoft.PowerShell shell).
    /// </summary>
    public string? ShellUri { get; init; }
}
