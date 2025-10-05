using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace CCEM.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly ISCCMConnectionService _connectionService;
    private readonly ISCCMPluginService _pluginService;
    private readonly IJsonNavigationService _navigationService;

    public ObservableCollection<PluginCategoryViewModel> PluginCategories { get; } = new();

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private string connectionStatusText = "Not connected";

    [ObservableProperty]
    private string? connectedHostname;

    [ObservableProperty]
    private string connectionStatusGlyph = "\uE702";

    [ObservableProperty]
    private Brush connectionStatusBrush = new SolidColorBrush(Colors.Gray);

    public MainViewModel(ISCCMConnectionService connectionService, ISCCMPluginService pluginService, IJsonNavigationService navigationService)
    {
        _connectionService = connectionService;
        _pluginService = pluginService;
        _navigationService = navigationService;

        InitializeConnectionState();
        LoadPlugins();

        _connectionService.ConnectionStatusChanged += OnConnectionStatusChanged;
    }

    [RelayCommand]
    private void NavigateToConnection()
    {
        _navigationService.NavigateTo("CCEM.Views.SCCM.ConnectionPage");
    }

    [RelayCommand(CanExecute = nameof(CanDisconnect))]
    private void Disconnect()
    {
        _connectionService.Disconnect();
    }

    public async Task ExecutePluginAsync(PluginShortcut plugin)
    {
        await _pluginService.ExecutePluginAsync(plugin.PluginName);
    }

    public void RefreshPlugins()
    {
        LoadPlugins();
    }

    private void LoadPlugins()
    {
        PluginCategories.Clear();

        foreach (var category in _pluginService.AvailableCategories.OrderBy(c => c, StringComparer.OrdinalIgnoreCase))
        {
            var categoryViewModel = new PluginCategoryViewModel(category);
            foreach (var pluginName in _pluginService.GetPluginsByCategory(category))
            {
                categoryViewModel.Plugins.Add(new PluginShortcut(category, pluginName, pluginName));
            }

            PluginCategories.Add(categoryViewModel);
        }
    }

    private void InitializeConnectionState()
    {
        UpdateConnectionIndicators(_connectionService.IsConnected, _connectionService.ConnectedHostname, null);
    }

    private void OnConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        UpdateConnectionIndicators(e.IsConnected, e.Hostname, e.ErrorMessage);
        DisconnectCommand.NotifyCanExecuteChanged();
    }

    private void UpdateConnectionIndicators(bool connected, string? hostname, string? error)
    {
        IsConnected = connected;
        ConnectedHostname = hostname;

        if (connected)
        {
            ConnectionStatusText = string.IsNullOrWhiteSpace(hostname) ? "Connected" : $"Connected to {hostname}";
            ConnectionStatusGlyph = "\uE701"; // PlugConnected
            ConnectionStatusBrush = new SolidColorBrush(Colors.ForestGreen);
        }
        else if (!string.IsNullOrWhiteSpace(error))
        {
            ConnectionStatusText = error;
            ConnectionStatusGlyph = "\uEA39"; // Error
            ConnectionStatusBrush = new SolidColorBrush(Colors.OrangeRed);
        }
        else
        {
            ConnectionStatusText = "Not connected";
            ConnectionStatusGlyph = "\uE702"; // PlugDisconnected
            ConnectionStatusBrush = new SolidColorBrush(Colors.Gray);
        }
    }

    private bool CanDisconnect() => _connectionService.IsConnected;

    public void Dispose()
    {
        _connectionService.ConnectionStatusChanged -= OnConnectionStatusChanged;
    }

    public sealed class PluginCategoryViewModel
    {
        public PluginCategoryViewModel(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public ObservableCollection<PluginShortcut> Plugins { get; } = new();
    }

    public readonly record struct PluginShortcut(string Category, string DisplayName, string PluginName);
}
