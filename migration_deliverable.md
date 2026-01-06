# Migration Deliverable: sccmclictr -> CCEM

Ce document sert de livrable final (mapping fonctionnalités, décisions d’architecture, checklist de migration, état d’avancement).

## 1) Inventaire fonctionnel (legacy)
- UI principale (WPF): `CCCM_source/SCCMCliCtrWPF/SCCMCliCtrWPF/MainPage.xaml`
  - Connexion à une machine cible (WinRM/WSMan + creds + port).
  - Exécution d’actions “Agent Actions” (inventaires, policy cycles, reset policy, location services, etc.).
  - Pages/onglets: Agent Settings, SettingsMgmt, ServiceWindow, InstalledSW, EventMonitoring, Advertisements, SWDistApps, Cache, Components, Software Distribution (summary), Services, Process, SWUpdates, All Updates, Execution History, InstallRepair, Collection Variables, CCMEval, PwrSettings, Log Monitoring, WMIBrowser, About.
- Plugins: scan `Plugin*.dll` et activation par convention de nom (AgentActionTool_*, CustomTools_*).

## 2) Cible d’architecture (CCEM)
- UI WinUI 3 (projet existant): `src/CCEM`
  - Navigation par module via `Assets/NavViewMenu/*.json` + `DevWinUI` (NavigationView + breadcrumb + search).
  - MVVM via `CommunityToolkit.Mvvm` + DI (`Microsoft.Extensions.DependencyInjection`).
- Domaine SCCM (à ajouter): **bibliothèque .NET 10** dédiée, injectable dans `CCEM`:
  - `CCEM.Core.Sccm` (nouveau): connexion, exécution d’actions, accès aux données SCCM client, modèles.
  - `CCEM.Core.Sccm.Automation` (dans `CCEM.Core.Sccm`): portage du code `sccmclictr.automation` (WSMan/WinRM + WMI + cache).
  - `CCEM` expose un `ISccmConnectionService` (stateful) pour partager l’agent connecté entre pages.

## 3) Mapping legacy -> CCEM
- Stratégie UI: remplacer le `TabControl` WPF par un ensemble de pages WinUI (NavigationView) sous le module SCCM.
- Mapping initial des onglets legacy -> pages WinUI (SCCM module):
  - Agent Settings -> `Sccm/Connection` (connexion + options) + `Sccm/Agent` (infos agent)
  - SettingsMgmt -> `Sccm/SettingsMgmt`
  - ServiceWindow -> `Sccm/ServiceWindows`
  - InstalledSW -> `Sccm/InstalledSoftware`
  - EventMonitoring -> `Sccm/EventMonitoring`
  - Advertisements -> `Sccm/Advertisements`
  - SWDistApps -> `Sccm/SoftwareDistributionApps`
  - Cache -> `Sccm/Cache`
  - Components -> `Sccm/Components`
  - Software Distribution (summary) -> `Sccm/SoftwareDistributionSummary`
  - Services -> `Sccm/Services`
  - Process -> `Sccm/Processes`
  - SWUpdates -> `Sccm/SoftwareUpdates`
  - All Updates -> `Sccm/AllUpdates`
  - Execution History -> `Sccm/ExecutionHistory`
  - InstallRepair -> `Sccm/InstallRepair`
  - Collection Variables -> `Sccm/CollectionVariables`
  - CCMEval -> `Sccm/CcmEval`
  - PwrSettings -> `Sccm/PowerSettings`
  - Log Monitoring -> `Sccm/Logs`
  - WMIBrowser -> `Sccm/WmiBrowser`
  - About -> `Settings/About` (déjà existant côté CCEM)
- Mapping plugins:
  - AgentActionTool_* -> commandes/actions contextuelles (toolbar/command bar) dans le module SCCM.
  - CustomTools_* -> section "Tools" (pages ou flyouts) dans le module SCCM.

## 4) Checklist de migration
- [x] Ajouter `CCEM.Core.Sccm` (port du backend automation en .NET 10)
- [x] Ajouter `ISccmConnectionService` (connexion/déconnexion)
- [x] Ajouter pages SCCM (squelette) + navigation JSON
- [x] Implémenter connexion + 2-3 écrans “socle” (Services/Processes/Cache) pour valider l’architecture
- [x] Migrer progressivement les écrans restants (tous les placeholders SCCM remplacés)
  - `SccmAgentSettingsPage`, `SccmSettingsMgmtPage`, `SccmServiceWindowsPage`, `SccmEventMonitoringPage`
  - `SccmAdvertisementsPage`, `SccmSoftwareDistributionAppsPage`, `SccmSoftwareDistributionSummaryPage`
  - `SccmAllUpdatesPage`, `SccmExecutionHistoryPage`, `SccmInstallRepairPage`, `SccmCollectionVariablesPage`
  - `SccmCcmEvalPage`, `SccmPowerSettingsPage`, `SccmComponentsPage`
- [x] Migrer/absorber les plugins (RDP, MSRA, Regedit, Explorer, etc.)
  - Entrée navigation: `Sccm.Tools` -> `src/CCEM/Views/Modules/SccmToolsPage.xaml`
  - Parité “plugins launchers”: RDP/MSRA/CompMgmt/MSInfo32/Explorer (shares) / Regedit (assist)
  - Parité “console SCCM”: CmRcViewer / ResourceExplorer / StatView (si console installée)
  - Parité “remédiation”: Enable PSRemoting via WMI (Win32_Process.Create)
  - Parité “Defender”: quick/full scan + enable/disable realtime (via `GetStringFromPS`)
  - Parité “PSScripts”: exécution de `.ps1` depuis `AppContext.BaseDirectory\\PSScripts`
- [ ] Stabiliser (logs, erreurs, perf) + build/publish
