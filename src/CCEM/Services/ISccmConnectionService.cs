using CCEM.Core.Sccm.Automation;

namespace CCEM.Services;

public interface ISccmConnectionService
{
    event EventHandler? ConnectionChanged;

    SCCMAgent? Agent { get; }
    bool IsConnected { get; }

    string? LastHostname { get; }
    int LastPort { get; }
    string? LastUsername { get; }
    bool LastUseEncryption { get; }

    Task ConnectAsync(SccmConnectionOptions options, CancellationToken cancellationToken = default);
    Task DisconnectAsync();
}

public sealed record SccmConnectionOptions(
    string Hostname,
    int Port = 5985,
    string? Username = null,
    string? Password = null,
    bool UseEncryption = false,
    bool PreAuthenticateIpc = true);
