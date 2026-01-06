# Notes: Migration sccmclictr -> CCEM

## Sources
- Code source legacy: `CCCM_source` (restauré depuis git)
- Code CCEM: `src/CCEM`

## Synthesized Findings

### CCEM (état actuel)
- App WinUI 3 minimaliste: shell + sélection de module + settings.
- Dépendances: `CommunityToolkit.Mvvm`, `DevWinUI`, DI via `Microsoft.Extensions.DependencyInjection`, `Microsoft.WindowsAppSDK`.
- Projet déjà structuré par dossiers: `Views`, `ViewModels`, `Services`, `Themes`, `Common`.
- Navigation: `NavigationView` pilotée par JSON (`src/CCEM/Assets/NavViewMenu/*.json`) + mappings générés T4 (`src/CCEM/T4Templates/*Mappings.cs`).
- Solution: `src/CCEM.slnx` (format XML) + `Directory.Build.props` centralise le `TargetFramework` (`net10.0-windows...`).

### Avancement migration (implémenté)
- Nouveau projet: `src/CCEM.Core.Sccm` (port .NET 10 du backend `sccmclictr.automation`) + dépendances (`System.Management`, `Microsoft.PowerShell.SDK`, `System.Runtime.Caching`).
- UI CCEM:
  - Service DI: `ISccmConnectionService` pour gérer l’agent connecté.
  - Pages SCCM ajoutées: `SccmConnectionPage` + pages SCCM (remplacement complet des placeholders).
  - Première page “socle” implémentée: `SccmServicesPage` (liste des services via `agent.Client.Services.Win32_Services`).
  - Deuxième page “socle” implémentée: `SccmProcessesPage` (liste des processes via `agent.Client.Process.ExtProcesses(true)`).
  - Pages supplémentaires implémentées:
    - `SccmCachePage` (cache SCCM + cleanup orphans + delete selected)
    - `SccmSoftwareUpdatesPage` (liste updates + install actions)
    - `SccmLogsPage` (lecture “tail” d’un fichier log distant)
    - `SccmWmiBrowserPage` (exécution de requêtes WQL + inspection propriétés)
    - `SccmAgentActionsPage` (déclenchement d’actions client: inventaires, policies, updates, reset policy)
    - `SccmInstalledSoftwarePage` (inventaire “Add/Remove Programs” via `SMS_InstalledSoftware`)
    - `SccmAgentSettingsPage` (infos agent / MP / site / paths)
    - `SccmSettingsMgmtPage` (diff requested vs actual pour `ComponentClientConfig`)
    - `SccmServiceWindowsPage` (liste + create/delete service windows)
    - `SccmEventMonitoringPage` (lecture du log `Microsoft-Windows-CCM/Operational`)
    - `SccmAdvertisementsPage` (liste + trigger/enforce)
    - `SccmSoftwareDistributionAppsPage` (liste apps + install/repair/uninstall/download/cancel)
    - `SccmComponentsPage` (liste des composants SCCM)
    - `SccmSoftwareDistributionSummaryPage` (summary software distribution)
    - `SccmAllUpdatesPage` (liste complète updates + install selected)
    - `SccmExecutionHistoryPage` (liste + delete selected)
    - `SccmInstallRepairPage` (repair/uninstall + resets)
    - `SccmCollectionVariablesPage` (liste + decode)
    - `SccmCcmEvalPage` (run + status)
    - `SccmPowerSettingsPage` (inventaire power settings)
  - Navigation SCCM enrichie via `src/CCEM/Assets/NavViewMenu/Sccm.json` + `src/CCEM/T4Templates/NavigationPageMappings.cs`.

### sccmclictr (à analyser)
- Sources disponibles localement: `CCCM_source/SCCMCliCtrWPF` + `CCCM_source/Plugins` + `CCCM_source/Customization`.

#### UI principale (WPF)
- Fenêtre `MainPage.xaml` avec:
  - Bandeau connexion (Target Computer + Connect via WinRM/WSMan).
  - Ribbon avec “Agent Actions” (inventaires, policy cycles, reset policy, etc.).
  - `TabControl` avec pages/onglets:
    - Agent Settings, SettingsMgmt, ServiceWindow, InstalledSW, EventMonitoring, Advertisements, SWDistApps, Cache, Components, Software Distribution (summary), Services, Process, SWUpdates, All Updates, Execution History, InstallRepair, Collection Variables, CCMEval, PwrSettings, Log Monitoring, WMIBrowser, About.

#### Plugins (chargement runtime)
- Chargement dynamique depuis `AppContext.BaseDirectory` (pattern `Plugin*.dll`) puis instanciation de types dont le nom commence par:
  - `AgentActionTool_` (ajout dans la zone outils du ribbon)
  - `CustomTools_` (autres outils/custom actions)
- Plugins présents dans `CCCM_source/Plugins`:
  - `Plugin_AppV46`, `Plugin_CompMgmt`, `Plugin_CustomTools_AMTTools`, `Plugin_EnablePSRemoting`, `Plugin_Explorer`, `Plugin_FEP`, `Plugin_MSInfo32`, `Plugin_MSRA`, `Plugin_PSScripts`, `Plugin_RDP`, `Plugin_Regedit`, `Plugin_RemoteTools`, `Plugin_ResourceExplorer`, `Plugin_RuckZuck`, `Plugin_SelfUpdate`, `Plugin_StatusMessageViewer`.

### Migration plugins -> CCEM (implémenté)
- Équivalent WinUI: une page `Sccm.Tools` pour regrouper les “plugins” sous forme d’outils natifs (launchers + scripts).
  - UI: `src/CCEM/Views/Modules/SccmToolsPage.xaml`
  - VM: `src/CCEM/ViewModels/Modules/SccmToolsViewModel.cs`
  - Helpers: `src/CCEM/Services/SccmConsoleLocator.cs` + mémorisation du dernier host dans `src/CCEM/Services/SccmConnectionService.cs`
- Outils portés (principaux):
  - Launchers: RDP (`mstsc`), MSRA (`msra /offerRA`), CompMgmt (`compmgmt.msc /computer`), MSInfo32 (`msinfo32 /computer`)
  - Explorer: `\\HOST\\C$`, `\\HOST\\Admin$`, `\\HOST\\[CCM Logs]` (utilise `LocalSCCMAgentLogPath` si connecté)
  - Console SCCM: CmRcViewer, ResourceExplorer (résolution SiteCode), StatView
  - Remédiation: Enable PSRemoting via WMI (Win32_Process.Create)
  - Defender: quick/full scan + enable/disable realtime
  - PSScripts: exécute des `.ps1` depuis `AppContext.BaseDirectory\\PSScripts` via `agent.Client.GetStringFromPS`

#### Backend (automation)
- WPF app consomme `sccmclictr.automation.dll` (NuGet `sccmclictrlib`), via `SCCMAgent`:
  - Transport principal: WSMan/WinRM (PowerShell Runspace).
  - IPC$ (mpr.dll) pour pré-auth si creds non intégrés.
- Sources upstream récupérées pour analyse: `legacy_upstream/sccmclictrlib` (projets `sccmclictr.automation` / `smsclictr.automation`).

##### sccmclictr.automation (structure)
- `SCCMAgent` (connexion WSMan + gestion Runspace + IPC$).
- `baseInit` (execution PowerShell + cache MemoryCache + TraceSource).
- `ccm` (façade qui expose `Client.*` regroupant des “features”):
  - `AgentProperties`, `AgentActions`, `SoftwareDistribution`, `SWCache`, `SoftwareUpdates`, `Inventory`, `Components`, `Services`, `Process`, `DCM`, `LocationServices`, `Monitoring`, `Health`, `AppV4`, `AppV5`, `RequestedConfig`, `ActualConfig`.
- Dépendances .NET 4.8 notables:
  - `System.Management` (WMI)
  - `System.Management.Automation` (PowerShell Runspaces/WSMan)
  - `System.Runtime.Caching` (cache)
