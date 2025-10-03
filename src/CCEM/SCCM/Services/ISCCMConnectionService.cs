namespace CCEM.SCCM.Services;

/// <summary>
/// Service interface for managing SCCM client connections
/// </summary>
public interface ISCCMConnectionService
{
    /// <summary>
    /// Gets the current SCCM agent instance
    /// </summary>
    Automation.SCCMAgent? CurrentAgent { get; }

    /// <summary>
    /// Gets a value indicating whether the service is connected to an SCCM client
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the hostname of the connected client
    /// </summary>
    string? ConnectedHostname { get; }

    /// <summary>
    /// Connects to a local or remote SCCM client
    /// </summary>
    /// <param name="hostname">The hostname to connect to (null or empty for local)</param>
    /// <param name="username">Optional username for remote connection</param>
    /// <param name="password">Optional password for remote connection</param>
    /// <returns>True if connection successful, false otherwise</returns>
    Task<bool> ConnectAsync(string? hostname = null, string? username = null, string? password = null);

    /// <summary>
    /// Disconnects from the current SCCM client
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Event raised when connection status changes
    /// </summary>
    event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;
}

/// <summary>
/// Event arguments for connection status changes
/// </summary>
public class ConnectionStatusChangedEventArgs : EventArgs
{
    public bool IsConnected { get; init; }
    public string? Hostname { get; init; }
    public string? ErrorMessage { get; init; }
}
