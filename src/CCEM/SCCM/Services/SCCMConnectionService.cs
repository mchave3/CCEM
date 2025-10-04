using CCEM.SCCM.Automation;

namespace CCEM.SCCM.Services;

/// <summary>
/// Implementation of SCCM connection service
/// </summary>
public class SCCMConnectionService : ISCCMConnectionService
{
    private SCCMAgent? _currentAgent;

    public SCCMAgent? CurrentAgent => _currentAgent;

    public bool IsConnected => _currentAgent?.isConnected ?? false;

    public string? ConnectedHostname => _currentAgent?.TargetHostname;

    public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

    /// <summary>
    /// Connects to a local or remote SCCM client
    /// </summary>
    public async Task<bool> ConnectAsync(string? hostname = null, string? username = null, string? password = null)
    {
        try
        {
            // Disconnect existing connection if any
            Disconnect();

            // Create new agent instance (run in background thread to avoid blocking UI)
            // SCCMAgent constructors auto-connect, so we just need to instantiate
            await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(hostname))
                {
                    // Local connection - use localhost
                    _currentAgent = new SCCMAgent("localhost");
                }
                else
                {
                    // Remote connection
                    if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                    {
                        _currentAgent = new SCCMAgent(hostname, username, password);
                    }
                    else
                    {
                        _currentAgent = new SCCMAgent(hostname);
                    }
                }
            });

            // Raise connection status changed event
            OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs
            {
                IsConnected = true,
                Hostname = ConnectedHostname
            });

            return IsConnected;
        }
        catch (Exception ex)
        {
            OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs
            {
                IsConnected = false,
                Hostname = hostname,
                ErrorMessage = ex.Message
            });

            return false;
        }
    }

    /// <summary>
    /// Disconnects from the current SCCM client
    /// </summary>
    public void Disconnect()
    {
        if (_currentAgent != null)
        {
            try
            {
                _currentAgent.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
            finally
            {
                _currentAgent = null;
                OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs
                {
                    IsConnected = false
                });
            }
        }
    }

    protected virtual void OnConnectionStatusChanged(ConnectionStatusChangedEventArgs e)
    {
        ConnectionStatusChanged?.Invoke(this, e);
    }
}
