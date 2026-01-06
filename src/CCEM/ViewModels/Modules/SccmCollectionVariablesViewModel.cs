using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Automation.policy;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record SccmCollectionVariableRow(
    string Name,
    string Value,
    actualConfig.CCM_CollectionVariable Source);

public sealed partial class SccmCollectionVariablesViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmCollectionVariablesViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmCollectionVariableRow> Variables { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                DecodeSelectedCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private string _decodedValue = string.Empty;
    public string DecodedValue
    {
        get => _decodedValue;
        private set => SetProperty(ref _decodedValue, value);
    }

    private SccmCollectionVariableRow? _selectedVariable;
    public SccmCollectionVariableRow? SelectedVariable
    {
        get => _selectedVariable;
        set
        {
            if (SetProperty(ref _selectedVariable, value))
            {
                DecodeSelectedCommand.NotifyCanExecuteChanged();
            }
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected)
        {
            StatusMessage = "Not connected.";
            Variables.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Variables.Clear();
        DecodedValue = string.Empty;

        try
        {
            var rows = await Task.Run(() =>
            {
                var list = agent.Client.ActualConfig.CollectionVariables;
                return list.Select(v => new SccmCollectionVariableRow(
                    Name: v.Name ?? string.Empty,
                    Value: v.Value ?? string.Empty,
                    Source: v)).ToList();
            }).ConfigureAwait(false);

            foreach (var row in rows.OrderBy(r => r.Name))
            {
                Variables.Add(row);
            }

            StatusMessage = $"Loaded {Variables.Count} variables.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load variables: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDecodeSelected))]
    private async Task DecodeSelectedAsync()
    {
        var selected = SelectedVariable;
        if (selected is null)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = "Decoding selected variable...";
        DecodedValue = string.Empty;

        try
        {
            var decoded = await Task.Run(() => selected.Source.DecodeValue()).ConfigureAwait(false);
            DecodedValue = decoded;
            StatusMessage = "Decoded.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Decode failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanDecodeSelected() => !IsLoading && SelectedVariable is not null;
}

