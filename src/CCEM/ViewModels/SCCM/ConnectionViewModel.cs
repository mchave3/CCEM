using System.Collections.ObjectModel;
using CCEM.SCCM.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace CCEM.ViewModels.SCCM;

/// <summary>
/// View model for SCCM client connection management
/// </summary>
public partial class ConnectionViewModel : ObservableObject
{
    private readonly ISCCMConnectionService _connectionService;

    #region Observable Properties

    [ObservableProperty]
    private string hostname = string.Empty;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isConnecting;

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private string statusMessage = "Not connected";

    [ObservableProperty]
    private InfoBarSeverity statusSeverity = InfoBarSeverity.Informational;

    [ObservableProperty]
    private bool showStatusMessage;

    [ObservableProperty]
    private string connectedHostname = "-";

    [ObservableProperty]
    private string agentVersion = "-";

    [ObservableProperty]
    private string siteCode = "-";

    [ObservableProperty]
    private string statusIcon = "\uE894"; // Disconnected icon

    [ObservableProperty]
    private SolidColorBrush statusColor = new(Colors.Gray);

    [ObservableProperty]
    private string connectionStatusText = "Disconnected";

    [ObservableProperty]
    private ObservableCollection<string> connectionHistory = new();

    #endregion

    #region Computed Properties

    /// <summary>
    /// Gets a value indicating whether the connection can be initiated
    /// </summary>
    public bool CanConnect => !IsConnecting && !IsConnected;

    /// <summary>
    /// Gets a value indicating whether connection history is available
    /// </summary>
    public bool HasConnectionHistory => ConnectionHistory.Count > 0;

    #endregion

    /// <summary>
    /// Initializes a new instance of the ConnectionViewModel class
    /// </summary>
    /// <param name="connectionService">SCCM connection service</param>
    public ConnectionViewModel(ISCCMConnectionService connectionService)
    {
        _connectionService = connectionService;
        _connectionService.ConnectionStatusChanged += OnConnectionStatusChanged;

        // Initialize with current connection state
        UpdateConnectionInfo();
    }

    #region Commands

    /// <summary>
    /// Connects to the SCCM client
    /// </summary>
    [RelayCommand]
    private async Task ConnectAsync()
    {
        IsConnecting = true;
        ShowStatusMessage = true;
        StatusMessage = "Connecting...";
        StatusSeverity = InfoBarSeverity.Informational;

        try
        {
            // Determine hostname to connect to
            string targetHostname = string.IsNullOrWhiteSpace(Hostname) ? null : Hostname.Trim();
            string targetUsername = string.IsNullOrWhiteSpace(Username) ? null : Username.Trim();
            string targetPassword = string.IsNullOrWhiteSpace(Password) ? null : Password;

            // Connect to SCCM client
            bool success = await _connectionService.ConnectAsync(targetHostname, targetUsername, targetPassword);

            if (success)
            {
                // Add to connection history if not localhost
                if (!string.IsNullOrWhiteSpace(targetHostname) &&
                    !targetHostname.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
                    !ConnectionHistory.Contains(targetHostname))
                {
                    ConnectionHistory.Insert(0, targetHostname);

                    // Keep only last 10 connections
                    while (ConnectionHistory.Count > 10)
                    {
                        ConnectionHistory.RemoveAt(ConnectionHistory.Count - 1);
                    }
                }

                StatusMessage = $"Successfully connected to {_connectionService.ConnectedHostname}";
                StatusSeverity = InfoBarSeverity.Success;

                // Clear password for security
                Password = string.Empty;
            }
            else
            {
                StatusMessage = "Failed to connect to SCCM client";
                StatusSeverity = InfoBarSeverity.Error;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Connection error: {ex.Message}";
            StatusSeverity = InfoBarSeverity.Error;
        }
        finally
        {
            IsConnecting = false;
            OnPropertyChanged(nameof(CanConnect));
            OnPropertyChanged(nameof(HasConnectionHistory));
        }
    }

    /// <summary>
    /// Disconnects from the current SCCM client
    /// </summary>
    [RelayCommand]
    private void Disconnect()
    {
        try
        {
            _connectionService.Disconnect();
            StatusMessage = "Disconnected from SCCM client";
            StatusSeverity = InfoBarSeverity.Informational;
            ShowStatusMessage = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Disconnect error: {ex.Message}";
            StatusSeverity = InfoBarSeverity.Warning;
            ShowStatusMessage = true;
        }
    }

    /// <summary>
    /// Clears the connection history
    /// </summary>
    [RelayCommand]
    private void ClearHistory()
    {
        ConnectionHistory.Clear();
        OnPropertyChanged(nameof(HasConnectionHistory));
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles connection status changes from the connection service
    /// </summary>
    private void OnConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        // Update connection state
        IsConnected = e.IsConnected;

        // Update UI on main thread
        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            UpdateConnectionInfo();

            if (!e.IsConnected)
            {
                // Show disconnection message
                if (!string.IsNullOrEmpty(e.ErrorMessage))
                {
                    StatusMessage = $"Connection error: {e.ErrorMessage}";
                    StatusSeverity = InfoBarSeverity.Error;
                    ShowStatusMessage = true;
                }
            }

            OnPropertyChanged(nameof(CanConnect));
        });
    }

    /// <summary>
    /// Updates connection information from the service
    /// </summary>
    private void UpdateConnectionInfo()
    {
        IsConnected = _connectionService.IsConnected;

        if (IsConnected)
        {
            ConnectedHostname = _connectionService.ConnectedHostname ?? "localhost";
            StatusIcon = "\uE8FB"; // Connected icon (CheckMark)
            StatusColor = new SolidColorBrush(Colors.Green);
            ConnectionStatusText = "Connected";

            // Get agent information if available
            try
            {
                var agent = _connectionService.CurrentAgent;
                if (agent != null)
                {
                    AgentVersion = agent.Client?.AgentProperties.ClientVersion ?? "-";
                    SiteCode = agent.Client?.AgentProperties.AssignedSite ?? "-";
                }
            }
            catch
            {
                AgentVersion = "-";
                SiteCode = "-";
            }
        }
        else
        {
            ConnectedHostname = "-";
            AgentVersion = "-";
            SiteCode = "-";
            StatusIcon = "\uE894"; // Disconnected icon (StatusCircleRing)
            StatusColor = new SolidColorBrush(Colors.Gray);
            ConnectionStatusText = "Disconnected";
        }
    }

    #endregion
}
