using CCEM.Core.Sccm.Automation;

namespace CCEM.Services;

public sealed class SccmConnectionService : ISccmConnectionService
{
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private SCCMAgent? _agent;
    private string? _lastHostname;
    private int _lastPort = 5985;
    private string? _lastUsername;
    private bool _lastUseEncryption;

    public event EventHandler? ConnectionChanged;

    public SCCMAgent? Agent => _agent;

    public bool IsConnected => _agent?.isConnected == true;

    public string? LastHostname => _lastHostname;
    public int LastPort => _lastPort;
    public string? LastUsername => _lastUsername;
    public bool LastUseEncryption => _lastUseEncryption;

    public async Task ConnectAsync(SccmConnectionOptions options, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.Hostname))
        {
            throw new ArgumentException("Hostname is required.", nameof(options));
        }

        if (options.Port is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Port must be between 1 and 65535.");
        }

        await _mutex.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _lastHostname = options.Hostname;
            _lastPort = options.Port;
            _lastUsername = string.IsNullOrWhiteSpace(options.Username) ? null : options.Username;
            _lastUseEncryption = options.UseEncryption;

            await DisconnectInternalAsync().ConfigureAwait(false);

            SCCMAgent agent;
            if (string.IsNullOrWhiteSpace(options.Username))
            {
                agent = new SCCMAgent(options.Hostname, options.Port, connect: false, encryption: options.UseEncryption);
            }
            else
            {
                agent = new SCCMAgent(options.Hostname, options.Username, options.Password ?? string.Empty, options.Port, Connect: false, encryption: options.UseEncryption);
            }

            if (!string.IsNullOrWhiteSpace(options.Username) && options.PreAuthenticateIpc)
            {
                agent.ConnectIPC(options.Username, options.Password ?? string.Empty);
            }

            await Task.Run(() => agent.connect(), cancellationToken).ConfigureAwait(false);

            _agent = agent;
        }
        finally
        {
            _mutex.Release();
        }

        ConnectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task DisconnectAsync()
    {
        await _mutex.WaitAsync().ConfigureAwait(false);
        try
        {
            await DisconnectInternalAsync().ConfigureAwait(false);
        }
        finally
        {
            _mutex.Release();
        }

        ConnectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private Task DisconnectInternalAsync()
    {
        try
        {
            if (_agent is { isConnected: true })
            {
                _agent.disconnect();
            }
        }
        catch
        {
        }

        try
        {
            _agent?.Dispose();
        }
        catch
        {
        }

        _agent = null;
        return Task.CompletedTask;
    }
}
