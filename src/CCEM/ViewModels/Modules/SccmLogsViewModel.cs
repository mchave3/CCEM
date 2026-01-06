using System.Collections.ObjectModel;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed partial class SccmLogsViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmLogsViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<string> Lines { get; } = new();

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

    private string _filePath = @"C:\Windows\CCM\Logs\ccmexec.log";
    public string FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }

    private int _tailLines = 200;
    public int TailLines
    {
        get => _tailLines;
        set => SetProperty(ref _tailLines, value);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected)
        {
            StatusMessage = "Not connected.";
            Lines.Clear();
            return;
        }

        if (string.IsNullOrWhiteSpace(FilePath))
        {
            StatusMessage = "File path is required.";
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Lines.Clear();

        try
        {
            var path = FilePath.Replace("'", "''");
            var tail = TailLines <= 0 ? 200 : TailLines;
            var ps = $"Get-Content -Path '{path}' -Tail {tail}";

            var contentLines = await Task.Run(() =>
            {
                var objs = agent.Client.GetObjectsFromPS(ps, Reload: true, tCacheTime: TimeSpan.Zero);
                return objs.Select(o => o?.ToString() ?? string.Empty).ToList();
            }).ConfigureAwait(false);

            foreach (var line in contentLines)
            {
                Lines.Add(line);
            }

            StatusMessage = $"Loaded {Lines.Count} lines.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load log: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

