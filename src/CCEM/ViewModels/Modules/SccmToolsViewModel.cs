using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management;
using CCEM.Services;
using Microsoft.UI.Dispatching;
using Windows.ApplicationModel.DataTransfer;

namespace CCEM.ViewModels.Modules;

public sealed record SccmToolRow(
    string Id,
    string Title,
    string Description,
    bool RequiresConnection);

public sealed record SccmScriptRow(string Name, string FullPath, string RelativePath);

public sealed partial class SccmToolsViewModel : ObservableObject
{
    private readonly ISccmConnectionService _connectionService;
    private readonly DispatcherQueue _dispatcherQueue;

    public SccmToolsViewModel(ISccmConnectionService connectionService)
    {
        _connectionService = connectionService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        Tools = new ObservableCollection<SccmToolRow>
        {
            new("rdp", "RDP", "Open Remote Desktop", RequiresConnection: false),
            new("msra", "MSRA", "Offer Remote Assistance", RequiresConnection: false),
            new("compmgmt", "Computer Management", "Open compmgmt.msc for target", RequiresConnection: false),
            new("msinfo32", "MSInfo32", "Open system information for target", RequiresConnection: false),
            new("explorer_c$", "Explorer: C$", "Open \\\\HOST\\C$", RequiresConnection: false),
            new("explorer_admin$", "Explorer: Admin$", "Open \\\\HOST\\Admin$", RequiresConnection: false),
            new("explorer_ccmlogs", "Explorer: CCM Logs", "Open SCCM client logs share", RequiresConnection: false),
            new("regedit", "Regedit", "Open Registry Editor (manual connect to remote)", RequiresConnection: false),
            new("cmremote", "CM Remote Control", "Open CmRcViewer.exe (requires ConfigMgr console)", RequiresConnection: false),
            new("resourceexplorer", "Resource Explorer", "Open ResourceExplorer.exe (requires ConfigMgr console)", RequiresConnection: false),
            new("statusmessages", "Status Messages", "Open statview.exe (requires ConfigMgr console)", RequiresConnection: false),
            new("enablepsremoting", "Enable PSRemoting", "Enable WinRM via remote WMI (Win32_Process.Create)", RequiresConnection: false),
            new("fep_quickscan", "Defender Quick Scan", "Start-MpScan -ScanType QuickScan -AsJob", RequiresConnection: true),
            new("fep_fullscan", "Defender Full Scan", "Start-MpScan -ScanType FullScan -AsJob", RequiresConnection: true),
            new("fep_realtime_on", "Defender Realtime: Enable", "Set-MpPreference -DisableRealtimeMonitoring $false", RequiresConnection: true),
            new("fep_realtime_off", "Defender Realtime: Disable", "Set-MpPreference -DisableRealtimeMonitoring $true", RequiresConnection: true),
            new("ruckzuck", "RuckZuck.tools", "Open https://ruckzuck.tools (legacy plugin replacement)", RequiresConnection: false),
        };
    }

    public ObservableCollection<SccmToolRow> Tools { get; }

    public ObservableCollection<SccmScriptRow> Scripts { get; } = new();

    private SccmToolRow? _selectedTool;
    public SccmToolRow? SelectedTool
    {
        get => _selectedTool;
        set
        {
            if (SetProperty(ref _selectedTool, value))
            {
                RunSelectedToolCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private SccmScriptRow? _selectedScript;
    public SccmScriptRow? SelectedScript
    {
        get => _selectedScript;
        set
        {
            if (SetProperty(ref _selectedScript, value))
            {
                RunSelectedScriptCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string _scriptRoot = string.Empty;
    public string ScriptRoot
    {
        get => _scriptRoot;
        set => SetProperty(ref _scriptRoot, value);
    }

    private string _scriptOutput = string.Empty;
    public string ScriptOutput
    {
        get => _scriptOutput;
        private set => SetProperty(ref _scriptOutput, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                RunSelectedToolCommand.NotifyCanExecuteChanged();
                RunSelectedScriptCommand.NotifyCanExecuteChanged();
                RefreshScriptsCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    [RelayCommand]
    private void EnsureScriptRoot()
    {
        if (!string.IsNullOrWhiteSpace(ScriptRoot))
        {
            return;
        }

        var defaultRoot = Path.Combine(AppContext.BaseDirectory, "PSScripts");
        ScriptRoot = defaultRoot;
        try
        {
            Directory.CreateDirectory(defaultRoot);
        }
        catch
        {
        }
    }

    [RelayCommand(CanExecute = nameof(CanRefreshScripts))]
    private async Task RefreshScriptsAsync()
    {
        EnsureScriptRoot();

        if (string.IsNullOrWhiteSpace(ScriptRoot) || !Directory.Exists(ScriptRoot))
        {
            StatusMessage = "Script folder not found.";
            Scripts.Clear();
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading scripts...";
        Scripts.Clear();

        try
        {
            var root = ScriptRoot;
            var rows = await Task.Run(() =>
            {
                var list = Directory.EnumerateFiles(root, "*.ps1", SearchOption.AllDirectories)
                    .Select(p =>
                    {
                        var relative = Path.GetRelativePath(root, p);
                        return new SccmScriptRow(
                            Name: Path.GetFileNameWithoutExtension(p),
                            FullPath: p,
                            RelativePath: relative);
                    })
                    .OrderBy(r => r.RelativePath, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return list;
            }).ConfigureAwait(false);

            foreach (var row in rows)
            {
                Scripts.Add(row);
            }

            StatusMessage = $"Loaded {Scripts.Count} scripts.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load scripts: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanRefreshScripts() => !IsLoading;

    [RelayCommand]
    private void OpenScriptsFolder()
    {
        EnsureScriptRoot();
        if (string.IsNullOrWhiteSpace(ScriptRoot))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{ScriptRoot}\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to open folder: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanRunSelectedScript))]
    private async Task RunSelectedScriptAsync()
    {
        var agent = _connectionService.Agent;
        var script = SelectedScript;
        if (agent is null || !agent.isConnected || script is null)
        {
            StatusMessage = "Not connected or no script selected.";
            return;
        }

        IsLoading = true;
        StatusMessage = $"Running: {script.RelativePath}";
        ScriptOutput = string.Empty;

        try
        {
            var output = await Task.Run(() =>
            {
                var text = File.ReadAllText(script.FullPath);
                return agent.Client.GetStringFromPS(text);
            }).ConfigureAwait(false);

            ScriptOutput = output ?? string.Empty;
            StatusMessage = "Script executed.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Script failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanRunSelectedScript() => !IsLoading && SelectedScript is not null;

    [RelayCommand(CanExecute = nameof(CanRunSelectedTool))]
    private Task RunSelectedToolAsync()
    {
        var tool = SelectedTool;
        if (tool is null)
        {
            return Task.CompletedTask;
        }

        return RunToolAsync(tool);
    }

    private bool CanRunSelectedTool()
    {
        if (IsLoading)
        {
            return false;
        }

        var tool = SelectedTool;
        if (tool is null)
        {
            return false;
        }

        if (tool.RequiresConnection)
        {
            return _connectionService.Agent?.isConnected == true;
        }

        return true;
    }

    [RelayCommand]
    private async Task RunToolAsync(SccmToolRow tool)
    {
        IsLoading = true;
        StatusMessage = $"Running: {tool.Title}";

        try
        {
            var target = GetTargetHostname();
            if (string.IsNullOrWhiteSpace(target))
            {
                StatusMessage = "No target hostname (connect or enter hostname in Connection page).";
                return;
            }

            switch (tool.Id)
            {
                case "rdp":
                    StartProcess("mstsc.exe", $"/v:{target}");
                    break;
                case "msra":
                    StartProcess("msra.exe", $"/offerRA {target}");
                    break;
                case "compmgmt":
                    StartProcess("compmgmt.msc", $"/computer:{target}");
                    break;
                case "msinfo32":
                    StartProcess("msinfo32.exe", $"/computer {target}");
                    break;
                case "explorer_c$":
                    StartProcess("explorer.exe", $@"\\{target}\C$");
                    break;
                case "explorer_admin$":
                    StartProcess("explorer.exe", $@"\\{target}\Admin$");
                    break;
                case "explorer_ccmlogs":
                    StartProcess("explorer.exe", GetCcmLogsSharePath(target));
                    break;
                case "regedit":
                    StartProcess("regedit.exe", null);
                    CopyToClipboard(target);
                    StatusMessage = $"Regedit opened. Hostname copied: {target}";
                    break;
                case "cmremote":
                    StartCmRemoteControl(target);
                    break;
                case "resourceexplorer":
                    await StartResourceExplorerAsync(target).ConfigureAwait(false);
                    break;
                case "statusmessages":
                    StartStatusMessageViewer(target);
                    break;
                case "enablepsremoting":
                    await EnablePsRemotingAsync(target).ConfigureAwait(false);
                    break;
                case "fep_quickscan":
                    await RunRemotePsAsync("Start-MpScan -ScanType QuickScan -AsJob").ConfigureAwait(false);
                    break;
                case "fep_fullscan":
                    await RunRemotePsAsync("Start-MpScan -ScanType FullScan -AsJob").ConfigureAwait(false);
                    break;
                case "fep_realtime_on":
                    await RunRemotePsAsync("Set-MpPreference -DisableRealtimeMonitoring $false").ConfigureAwait(false);
                    break;
                case "fep_realtime_off":
                    await RunRemotePsAsync("Set-MpPreference -DisableRealtimeMonitoring $true").ConfigureAwait(false);
                    break;
                case "ruckzuck":
                    StartProcess("explorer.exe", "https://ruckzuck.tools");
                    break;
                default:
                    StatusMessage = $"Unknown tool: {tool.Id}";
                    break;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Tool failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string? GetTargetHostname()
    {
        var agent = _connectionService.Agent;
        if (agent is not null && !string.IsNullOrWhiteSpace(agent.TargetHostname))
        {
            return agent.TargetHostname;
        }

        return _connectionService.LastHostname;
    }

    private static void StartProcess(string fileName, string? args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = true,
        };

        if (!string.IsNullOrWhiteSpace(args))
        {
            startInfo.Arguments = args;
        }

        Process.Start(startInfo);
    }

    private string GetCcmLogsSharePath(string target)
    {
        var agent = _connectionService.Agent;
        if (agent is { isConnected: true })
        {
            var logPath = agent.Client.AgentProperties.LocalSCCMAgentLogPath;
            if (!string.IsNullOrWhiteSpace(logPath) && logPath.Contains(':'))
            {
                var unc = logPath.Replace(':', '$');
                return $@"\\{target}\{unc}";
            }
        }

        return $@"\\{target}\Admin$\ccm\logs";
    }

    private void CopyToClipboard(string text)
    {
        try
        {
            var data = new DataPackage();
            data.SetText(text);
            Clipboard.SetContent(data);
        }
        catch
        {
        }
    }

    private void StartCmRemoteControl(string target)
    {
        var info = SccmConsoleLocator.TryGetConsoleInfo();
        if (info is null)
        {
            StatusMessage = "ConfigMgr console not found (UI Installation Directory missing).";
            return;
        }

        var exePath = Path.Combine(info.UiInstallationDirectory, "bin", "i386", "CmRcViewer.exe");
        if (!File.Exists(exePath))
        {
            StatusMessage = "CmRcViewer.exe not found in ConfigMgr console.";
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = Path.GetDirectoryName(exePath),
            Arguments = target,
            UseShellExecute = true
        });
    }

    private void StartStatusMessageViewer(string target)
    {
        var info = SccmConsoleLocator.TryGetConsoleInfo();
        if (info is null)
        {
            StatusMessage = "ConfigMgr console not found (UI Installation Directory missing).";
            return;
        }

        var exePath = Path.Combine(info.UiInstallationDirectory, "bin", "i386", "statview.exe");
        if (!File.Exists(exePath))
        {
            StatusMessage = "statview.exe not found in ConfigMgr console.";
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = Path.GetDirectoryName(exePath),
            Arguments = $"/SMS:SYSTEM={target}",
            UseShellExecute = true
        });
    }

    private async Task StartResourceExplorerAsync(string target)
    {
        var info = SccmConsoleLocator.TryGetConsoleInfo();
        if (info is null)
        {
            StatusMessage = "ConfigMgr console not found (UI Installation Directory missing).";
            return;
        }

        var exePath = Path.Combine(info.UiInstallationDirectory, "bin", "ResourceExplorer.exe");
        if (!File.Exists(exePath))
        {
            StatusMessage = "ResourceExplorer.exe not found in ConfigMgr console.";
            return;
        }

        var server = info.ConnectedServer?.Replace("\\", "").Trim();
        if (string.IsNullOrWhiteSpace(server))
        {
            StatusMessage = "ConfigMgr console server not found (AdminUI\\Connection\\Server).";
            return;
        }

        var siteCode = await Task.Run(() => TryGetSiteCode(server)).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(siteCode))
        {
            StatusMessage = "Failed to resolve SiteCode from SMS_ProviderLocation.";
            return;
        }

        var hostShort = target.Split('.')[0];
        var args = $"-s -sms:ResExplrQuery=\"SELECT ResourceID FROM SMS_R_SYSTEM WHERE Name = '{hostShort}'\" -sms:Connection=\\\\{server}\\root\\sms\\site_{siteCode}";

        Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = Path.GetDirectoryName(exePath),
            Arguments = args,
            UseShellExecute = true
        });
    }

    private static string? TryGetSiteCode(string server)
    {
        try
        {
            var scope = new ManagementScope($@"\\{server}\root\sms");
            scope.Connect();

            using var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM SMS_ProviderLocation"));
            foreach (ManagementObject obj in searcher.Get())
            {
                return obj["SiteCode"]?.ToString();
            }
        }
        catch
        {
        }

        return null;
    }

    private async Task EnablePsRemotingAsync(string target)
    {
        StatusMessage = "Enabling PSRemoting via WMI...";

        var ok = await Task.Run(() =>
        {
            try
            {
                var scope = new ManagementScope($@"\\{target}\root\cimv2");
                scope.Connect();

                using var processClass = new ManagementClass(scope, new ManagementPath("Win32_Process"), null);
                using var inParams = processClass.GetMethodParameters("Create");

                inParams["CommandLine"] =
                    "\"C:\\\\Windows\\\\system32\\\\WindowsPowerShell\\\\v1.0\\\\powershell.exe\" \"Enable-PSRemoting -Force\"";

                using var outParams = processClass.InvokeMethod("Create", inParams, null);
                var returnValue = outParams?["ReturnValue"];
                return returnValue is uint u && u == 0;
            }
            catch
            {
                return false;
            }
        }).ConfigureAwait(false);

        StatusMessage = ok
            ? "WinRM enabled (WMI). Try connecting again."
            : "Failed to enable WinRM via WMI (check firewall/permissions).";
    }

    private async Task RunRemotePsAsync(string ps)
    {
        var agent = _connectionService.Agent;
        if (agent is null || !agent.isConnected)
        {
            StatusMessage = "Not connected.";
            return;
        }

        await Task.Run(() => agent.Client.GetStringFromPS(ps)).ConfigureAwait(false);
        StatusMessage = "Command executed.";
    }
}
