using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml.Controls;

namespace CCEM.ViewModels.SCCM;

public partial class ConnectionViewModel : ObservableObject, IDisposable
{
    private const int MaxRecentHosts = 10;

    private readonly ISCCMConnectionService _connectionService;
    private readonly ObservableCollection<string> _recentHosts;
    private string? _password;

    public ObservableCollection<string> RecentHosts => _recentHosts;

    [ObservableProperty]
    private string targetHost = "localhost";

    [ObservableProperty]
    private string? userName;

    [ObservableProperty]
    private bool useAlternateCredentials;

    [ObservableProperty]
    private bool rememberHost = true;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private string connectionSummary = "Not connected.";

    [ObservableProperty]
    private bool isStatusVisible;

    [ObservableProperty]
    private string statusTitle = "Ready";

    [ObservableProperty]
    private string statusMessage = "Connect to a Configuration Manager client to begin managing it.";

    [ObservableProperty]
    private InfoBarSeverity statusSeverity = InfoBarSeverity.Informational;

    public ConnectionViewModel(ISCCMConnectionService connectionService)
    {
        _connectionService = connectionService;
        _recentHosts = new ObservableCollection<string>();

        InitializeFromService();
        _connectionService.ConnectionStatusChanged += OnConnectionStatusChanged;
    }

    [RelayCommand(CanExecute = nameof(CanConnectAsync))]
    private async Task ConnectAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        UpdateStatus("Connecting", $"Attempting to connect to {GetDisplayHost()}...", InfoBarSeverity.Informational, true);

        var host = string.IsNullOrWhiteSpace(TargetHost) ? null : TargetHost.Trim();
        var user = UseAlternateCredentials ? UserName : null;
        var password = UseAlternateCredentials ? _password : null;

        try
        {
            var connected = await _connectionService.ConnectAsync(host, user, password);

            if (connected)
            {
                if (RememberHost && !string.IsNullOrWhiteSpace(_connectionService.ConnectedHostname))
                {
                    AddHostToHistory(_connectionService.ConnectedHostname!);
                }

                UpdateStatus("Connected", $"Connected to {_connectionService.ConnectedHostname ?? GetDisplayHost()}", InfoBarSeverity.Success, true);
            }
            else
            {
                UpdateStatus("Connection failed", "Unable to connect to the requested device.", InfoBarSeverity.Error, true);
            }
        }
        catch (Exception ex)
        {
            UpdateStatus("Connection error", ex.Message, InfoBarSeverity.Error, true);
        }
        finally
        {
            IsBusy = false;
            ConnectCommand.NotifyCanExecuteChanged();
            DisconnectCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private void Disconnect()
    {
        _connectionService.Disconnect();
        UpdateStatus("Disconnected", "The client connection has been closed.", InfoBarSeverity.Informational, true);
        ConnectCommand.NotifyCanExecuteChanged();
        DisconnectCommand.NotifyCanExecuteChanged();
    }

    public void UpdatePassword(string? password)
    {
        _password = password;
    }

    private void InitializeFromService()
    {
        if (_connectionService.IsConnected)
        {
            IsConnected = true;
            ConnectionSummary = $"Connected to {_connectionService.ConnectedHostname}";
            TargetHost = _connectionService.ConnectedHostname ?? TargetHost;
            UpdateStatus("Connected", ConnectionSummary, InfoBarSeverity.Success, false);
        }
        else
        {
            IsConnected = false;
            ConnectionSummary = "Not connected.";
        }
    }

    private void OnConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        IsConnected = e.IsConnected;

        if (e.IsConnected)
        {
            var hostname = e.Hostname ?? GetDisplayHost();
            ConnectionSummary = $"Connected to {hostname}";
            UpdateStatus("Connected", ConnectionSummary, InfoBarSeverity.Success, false);
        }
        else
        {
            ConnectionSummary = "Not connected.";
            if (!string.IsNullOrEmpty(e.ErrorMessage))
            {
                UpdateStatus("Connection failed", e.ErrorMessage, InfoBarSeverity.Error, true);
            }
            else
            {
                UpdateStatus("Disconnected", "The client connection has been closed.", InfoBarSeverity.Informational, false);
            }
        }

        ConnectCommand.NotifyCanExecuteChanged();
        DisconnectCommand.NotifyCanExecuteChanged();
    }

    private void AddHostToHistory(string hostname)
    {
        if (string.IsNullOrWhiteSpace(hostname))
        {
            return;
        }

        if (_recentHosts.Any(item => string.Equals(item, hostname, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _recentHosts.Insert(0, hostname);
        while (_recentHosts.Count > MaxRecentHosts)
        {
            _recentHosts.RemoveAt(_recentHosts.Count - 1);
        }
    }

    private void UpdateStatus(string title, string message, InfoBarSeverity severity, bool isVisible)
    {
        StatusTitle = title;
        StatusMessage = message;
        StatusSeverity = severity;
        IsStatusVisible = isVisible;
    }

    private bool CanConnectAsync()
    {
        return !IsBusy;
    }

    private bool CanDisconnect()
    {
        return !IsBusy && IsConnected;
    }

    private string GetDisplayHost() => string.IsNullOrWhiteSpace(TargetHost) ? "localhost" : TargetHost.Trim();

    partial void OnIsBusyChanged(bool value)
    {
        ConnectCommand.NotifyCanExecuteChanged();
        DisconnectCommand.NotifyCanExecuteChanged();
    }

    partial void OnUseAlternateCredentialsChanged(bool value)
    {
        if (!value)
        {
            UserName = string.Empty;
            _password = string.Empty;
        }
    }

    partial void OnIsConnectedChanged(bool value)
    {
        if (!value)
        {
            if (!string.IsNullOrWhiteSpace(TargetHost))
            {
                ConnectionSummary = $"Ready to connect to {GetDisplayHost()}";
            }
        }
    }

    public void Dispose()
    {
        _connectionService.ConnectionStatusChanged -= OnConnectionStatusChanged;
    }
}
