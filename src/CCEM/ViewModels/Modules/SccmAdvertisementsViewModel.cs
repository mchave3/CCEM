using System.Collections.ObjectModel;
using CCEM.Core.Sccm.Automation.functions;
using CCEM.Services;

namespace CCEM.ViewModels.Modules;

public sealed record SccmAdvertisementRow(
    string AdvertisementId,
    string? ProgramName,
    string? PackageId,
    DateTime? ActiveTime,
    DateTime? ExpirationTime,
    bool? Mandatory,
    softwaredistribution.CCM_SoftwareDistribution Source);

public sealed partial class SccmAdvertisementsViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;

    public SccmAdvertisementsViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public ObservableCollection<SccmAdvertisementRow> Advertisements { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                TriggerSelectedCommand.NotifyCanExecuteChanged();
                EnforceSelectedCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private SccmAdvertisementRow? _selectedAdvertisement;
    public SccmAdvertisementRow? SelectedAdvertisement
    {
        get => _selectedAdvertisement;
        set
        {
            if (SetProperty(ref _selectedAdvertisement, value))
            {
                TriggerSelectedCommand.NotifyCanExecuteChanged();
                EnforceSelectedCommand.NotifyCanExecuteChanged();
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
            Advertisements.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading...";
        Advertisements.Clear();

        try
        {
            var rows = await Task.Run(() =>
            {
                var list = agent.Client.SoftwareDistribution.Advertisements;
                return list.Select(a => new SccmAdvertisementRow(
                    AdvertisementId: a.ADV_AdvertisementID ?? string.Empty,
                    ProgramName: a.PRG_ProgramName,
                    PackageId: a.PKG_PackageID,
                    ActiveTime: a.ADV_ActiveTime,
                    ExpirationTime: a.ADV_ExpirationTime,
                    Mandatory: a.ADV_MandatoryAssignments,
                    Source: a)).ToList();
            }).ConfigureAwait(false);

            foreach (var row in rows.OrderBy(r => r.ProgramName).ThenBy(r => r.AdvertisementId))
            {
                Advertisements.Add(row);
            }

            StatusMessage = $"Loaded {Advertisements.Count} advertisements.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load advertisements: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanTriggerSelected))]
    private async Task TriggerSelectedAsync()
    {
        var selected = SelectedAdvertisement;
        if (selected is null)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = "Triggering selected advertisement...";

        try
        {
            await Task.Run(() => selected.Source.TriggerSchedule(false)).ConfigureAwait(false);
            StatusMessage = "Trigger requested.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Trigger failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanTriggerSelected))]
    private async Task EnforceSelectedAsync()
    {
        var selected = SelectedAdvertisement;
        if (selected is null)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = "Enforcing and triggering selected advertisement...";

        try
        {
            await Task.Run(() => selected.Source.TriggerSchedule(true)).ConfigureAwait(false);
            StatusMessage = "Enforce + trigger requested.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Enforce failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanTriggerSelected() => !IsLoading && SelectedAdvertisement is not null;
}

