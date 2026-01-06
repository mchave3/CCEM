using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public partial class SccmConnectionViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmConnectionViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
        _connectionService.ConnectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(ConnectedHostname));
        };
    }

    public bool IsConnected => _connectionService.IsConnected;
    public string? ConnectedHostname => _connectionService.Agent?.TargetHostname;

    private string _hostname = string.Empty;
    public string Hostname
    {
        get => _hostname;
        set
        {
            if (SetProperty(ref _hostname, value))
            {
                ConnectCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private int _port = 5985;
    public int Port
    {
        get => _port;
        set
        {
            if (SetProperty(ref _port, value))
            {
                ConnectCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private bool _useEncryption;
    public bool UseEncryption
    {
        get => _useEncryption;
        set
        {
            if (SetProperty(ref _useEncryption, value))
            {
                ConnectCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                ConnectCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                ConnectCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                ConnectCommand.NotifyCanExecuteChanged();
                DisconnectCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        IsBusy = true;
        StatusMessage = "Connecting...";

        try
        {
            await _connectionService.ConnectAsync(new SccmConnectionOptions(
                Hostname: Hostname,
                Port: Port,
                Username: string.IsNullOrWhiteSpace(Username) ? null : Username,
                Password: string.IsNullOrWhiteSpace(Username) ? null : Password,
                UseEncryption: UseEncryption)).ConfigureAwait(false);

            StatusMessage = "Connected.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Connection failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            ConnectCommand.NotifyCanExecuteChanged();
            DisconnectCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(ConnectedHostname));
        }
    }

    private bool CanConnect() => !IsBusy && !IsConnected;

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private async Task DisconnectAsync()
    {
        IsBusy = true;
        StatusMessage = "Disconnecting...";

        try
        {
            await _connectionService.DisconnectAsync().ConfigureAwait(false);
            StatusMessage = "Disconnected.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Disconnect failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            ConnectCommand.NotifyCanExecuteChanged();
            DisconnectCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(ConnectedHostname));
        }
    }

    private bool CanDisconnect() => !IsBusy && IsConnected;
}
