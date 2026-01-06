using System.Collections.ObjectModel;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record SccmAgentPropertyRow(string Name, string Value);

public sealed partial class SccmAgentSettingsViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmAgentSettingsViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmAgentPropertyRow> Properties { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected)
        {
            StatusMessage = "Not connected.";
            Properties.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Properties.Clear();

        try
        {
            var rows = await Task.Run(() =>
            {
                static void Add(List<SccmAgentPropertyRow> list, string name, Func<string?> getter)
                {
                    try
                    {
                        list.Add(new SccmAgentPropertyRow(name, getter() ?? string.Empty));
                    }
                    catch (Exception ex)
                    {
                        list.Add(new SccmAgentPropertyRow(name, $"<error: {ex.Message}>"));
                    }
                }

                var list = new List<SccmAgentPropertyRow>
                {
                    new("Target", agent.TargetHostname ?? string.Empty),
                    new("Connected", agent.isConnected.ToString()),
                };

                Add(list, "ClientVersion", () => agent.Client.AgentProperties.ClientVersion);
                Add(list, "AssignedSite", () => agent.Client.AgentProperties.AssignedSite);
                Add(list, "ManagementPoint", () => agent.Client.AgentProperties.ManagementPoint);
                Add(list, "InternetMP", () => agent.Client.AgentProperties.ManagementPointInternet);
                Add(list, "ProductCode", () => agent.Client.AgentProperties.ProductCode);
                Add(list, "LocalAgentPath", () => agent.Client.AgentProperties.LocalSCCMAgentPath);
                Add(list, "LogsPath", () => agent.Client.AgentProperties.LocalSCCMAgentLogPath);

                return list;
            }).ConfigureAwait(false);

            foreach (var row in rows)
            {
                Properties.Add(row);
            }

            StatusMessage = $"Loaded {Properties.Count} properties.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load properties: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
