using System.Collections.ObjectModel;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record WmiPropertyRow(string Name, string Value);
public sealed record WmiResultRow(string Display, IReadOnlyList<WmiPropertyRow> Properties);

public sealed partial class SccmWmiBrowserViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmWmiBrowserViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<WmiResultRow> Results { get; } = new();
    public ObservableCollection<WmiPropertyRow> SelectedProperties { get; } = new();

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

    private string _wmiNamespace = @"root\cimv2";
    public string WmiNamespace
    {
        get => _wmiNamespace;
        set => SetProperty(ref _wmiNamespace, value);
    }

    private string _wqlQuery = "SELECT * FROM Win32_OperatingSystem";
    public string WqlQuery
    {
        get => _wqlQuery;
        set => SetProperty(ref _wqlQuery, value);
    }

    private bool _useCim = true;
    public bool UseCim
    {
        get => _useCim;
        set => SetProperty(ref _useCim, value);
    }

    private WmiResultRow? _selectedResult;
    public WmiResultRow? SelectedResult
    {
        get => _selectedResult;
        set
        {
            if (SetProperty(ref _selectedResult, value))
            {
                SelectedProperties.Clear();
                if (value is not null)
                {
                    foreach (var p in value.Properties)
                    {
                        SelectedProperties.Add(p);
                    }
                }
            }
        }
    }

    [RelayCommand]
    private async Task RunQueryAsync()
    {
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected)
        {
            StatusMessage = "Not connected.";
            Results.Clear();
            SelectedProperties.Clear();
            return;
        }

        if (string.IsNullOrWhiteSpace(WmiNamespace) || string.IsNullOrWhiteSpace(WqlQuery))
        {
            StatusMessage = "Namespace and query are required.";
            return;
        }

        IsLoading = true;
        StatusMessage = "Querying...";
        Results.Clear();
        SelectedProperties.Clear();
        SelectedResult = null;

        try
        {
            var ns = WmiNamespace.Trim();
            var query = WqlQuery.Trim();

            var rows = await Task.Run(() =>
            {
                var objs = UseCim
                    ? agent.Client.GetCimObjects(ns, query, Reload: true)
                    : agent.Client.GetObjects(ns, query, Reload: true);

                return objs.Select(o => ToRow(o)).ToList();
            }).ConfigureAwait(false);

            foreach (var row in rows)
            {
                Results.Add(row);
            }

            StatusMessage = $"Returned {Results.Count} objects.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Query failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static WmiResultRow ToRow(System.Management.Automation.PSObject obj)
    {
        string display =
            TryGetString(obj, "Name") ??
            TryGetString(obj, "Caption") ??
            TryGetString(obj, "__RELPATH") ??
            obj.ToString();

        var props = new List<WmiPropertyRow>();
        foreach (var p in obj.Properties)
        {
            var value = p.Value is null ? string.Empty : p.Value.ToString();
            props.Add(new WmiPropertyRow(p.Name, value ?? string.Empty));
        }

        return new WmiResultRow(display, props);
    }

    private static string? TryGetString(System.Management.Automation.PSObject obj, string propertyName)
    {
        try
        {
            var m = obj.Members[propertyName];
            return m?.Value?.ToString();
        }
        catch
        {
            return null;
        }
    }
}

