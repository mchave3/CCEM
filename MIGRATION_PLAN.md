# Migration Plan CCCM_source -> CCEM (.NET 10 / WinUI 3)

Statuses: `[ ]` Not started / `[~]` In progress / `[x]` Done / `[!]` Blocked

## Goal and scope
- Merge CCCM (SCCM) capabilities into CCEM (WinUI 3) and add Intune management.
- Modernize the stack (.NET 10, Windows App SDK/WinUI 3) without losing SCCM troubleshooting depth.
- Preserve remote ops (WinRM/WMI/PowerShell) and add Microsoft Graph calls for Intune.
- Adapt CCCM UX to CCEM patterns (JSON navigation, dynamic theming, Velopack packaging).

## WinUI 3 references (Context7)
- WinUI 3 ships via Windows App SDK (latest stable channel 1.4) and targets Desktop/Win32 apps (source: /microsoft/microsoft-ui-xaml).
- Porting requires replacing WPF controls with WinUI 3 equivalents and following the AppWindow/Window model (WinUI 3, Windows App SDK).

## Current state
### CCEM (target)
- WinUI 3, DI (App.ConfigureServices), JSON navigation, theming, packaging/updates via Velopack, DevWinUI + Toolkit animations.
- Main shell: `MainWindow.xaml` (TitleBar/NavView/Frame), services: `ThemeService`, `JsonNavigationService`, `ContextMenuService`, `VelopackUpdateService`.
### CCCM_source (origin)
- WPF .NET Framework 4.7, dependencies WinRM + PowerShell 4 + Configuration Manager Agent.
- Key modules (Controls): Advertisement/Application grids, CacheGrid, CCMEvalGrid, CollectionVariables, EventMonitoring, ExecHistory, InstallAgent/Repair, InstalledSoftwareGrid, LogViewer, PowerSettings, ProcessGrid, ServicesGrid, ServiceWindowGrid, SettingsMgmt, Software Updates (SW*), WMIBrowser, ScheduleControl.
- Config/infra: `Settings.cs`, `Logs.cs`, `app.config`, plugins (Plugins/Customization), resx/ico, packages.config.

## Risks and constraints
- SCCM-specific APIs (WMI/COM/registry/PowerShell) need revalidation on .NET 10 + WinUI 3 (interop and UAC/WinRM permissions).
- WPF -> WinUI 3 differences (resources, styles, behaviors, NavigationService) require UI/command refactors.
- Intune needs Microsoft Graph (interactive/device code auth, DeviceManagementConfiguration/ManagedDevices scopes) and token cache handling.
- Packaging: align MSIX/Velopack with native dependencies (WinRM, WebView2 if used).

## Migration roadmap (checkpoints)
### Phase 0 - Preparation and framing
- [x] P0.1 Draft initial plan and tracking rules.
- [!] P0.2 Run CCCM_source to capture expected behaviors (screenshots, logs, network flows).
- [x] P0.3 List technical dependencies (NuGet packages, COM/WMI interop, PS scripts) and WinUI/WinAppSDK equivalents.

Phase 0 notes (current):
- Dependencies captured: packages NavigationPane 2.1.0.0, sccmclictrlib 1.0.1, WPFToolkit 3.5.50211.1; references include System.Management.Automation 3.0, System.Management (WMI), PresentationFramework/WindowsFormsIntegration, WCF metadata. `app.config` embeds PowerShell scripts (WinRM/WMI polling, agent install), SCCM service list, COM DLL registration list, WinRM defaults (port 5985, SSL false), and default event query.
- CCCM_source run pending: needs build/run of the .NET Framework 4.8 WPF app on Windows with WinRM/PowerShell 4+ available; not executed in this session (manual run needed for screenshots/logs/flows).

### Phase 1 - Functional mapping and target design
- [x] P1.1 Map each CCCM module (UI + logic) and prioritize (core troubleshooting first: inventory, logs, services, updates).
- [x] P1.2 Define SCCM service layer (abstractions for WMI/PowerShell/WinRM) and Intune surface (Graph) exposed via interfaces.
- [x] P1.3 Define target navigation in CCEM (pages, JSON routes, breadcrumbs, search/title bar) to host migrated modules.
- [x] P1.4 Design unified settings model (SCCM + Intune) and persistence (app config -> modernized Settings).

Phase 1 notes (current):
- Module map (from MainPage): Monitoring (Log Monitoring, EventMonitoring), Inventory (Overview, ComponentsPanel, CachePanel, InstalledSWTab, Process, ServicesPanel, CCMEval, WMIBrowser), Software Distribution (SWDistSummary, AdvertisementsTab, SWDistApps, SWUpdates, SWAllUpdates, ExecHistory, CollectionVariables), Agent Settings (AgentSettingsPanel, SettingsMgmtPanel, ServiceWindowTab/ServiceWindowGrid, PwrSettingsPanel), Maintenance (InstallRepair, InstallAgent), About, Tools (LogViewer, PowerShell console), ribbons for Inventory/Policy/Updates/PowerShell/Maintenance/App.Mgmt.
- Priority for first deliverable: Inventory + Logs + Services + Updates + Process (core troubleshooting); second: Software Distribution + ExecHistory + CollectionVariables; third: Maintenance/Install + ServiceWindow + Power settings; plugins/custom actions deferred.
- SCCM service layer design: `IWmiQueryService` (typed queries for CCM_* classes and CIM inventory), `IPowerShellRemotingService` (WinRM remote exec with cancellation/progress), `ISccmClientService` (agent state/actions: eval, cache, components, logs, services, processes), `ISccmSoftwareDistributionService` (advertisements, apps, content status, exec history), `ISccmUpdateService` (SWUpdates/SWAllUpdates/SWStatus), `ILogCaptureService` (live log tailing + filters), `IServiceWindowService` (read/write service windows). Compose via DI and expose async APIs; shield UI from WMI/PS details.
- Intune surface design: `IIntuneAuthService` (device code or WAM + cache), `IIntuneDeviceService` (managed devices, compliance, primary users), `IIntuneAppService` (apps/assignments), `IIntuneUpdateService` (updates/quality/feature rings), shared models aligned with SCCM views for parity.
- Navigation plan (CCEM NavView/Frame): top-level items Monitoring, Inventory, Software Distribution, Agent Settings, Maintenance, Settings, About; each route stored in JSON nav config (e.g., `inv/overview`, `inv/cache`, `inv/components`, `inv/installed-sw`, `ops/processes`, `ops/services`, `updates/software`, `updates/all`, `updates/history`, `dist/ads`, `dist/apps`, `dist/summary`, `logs/live`, `logs/event-monitor`, `settings/agent`, `settings/service-window`, `settings/power`, `maint/install-repair`, `maint/install-agent`, `tools/wmi-browser`, `tools/collection-vars`). Breadcrumbs and search wired to tags; reuse CCEM TitleBar search hook.
- Settings model plan: single settings store with sections `Connection` (host, WinRM port, useSsl, creds), `Sccm` (site code, MP/FSP, highlighted services list, register DLL list, adhoc inventory queries, event query), `Intune` (scopes, tenant, cache), `Logs` (path, level, retention), `Updates` (channels). Persist to file (JSON) with secure cred handling; migrate legacy app.config values where applicable.

### Phase 2 - WinUI 3 / .NET 10 foundation
- [ ] P2.1 Create interop projects/libs (SccmClientSdk) isolating WMI/PowerShell/WinRM with DI and cancellation.
- [ ] P2.2 Add a logging adapter (Serilog already present via Core.Logger) + optional telemetry.
- [ ] P2.3 Port helpers (Converters, Logs, Settings) to WinUI 3 + INotifyPropertyChanged/MVVM (CommunityToolkit MVVM if added).
- [ ] P2.4 Integrate a remote execution service (PSRemoting/WinRM) with async/await and responsive UI.

### Phase 3 - SCCM feature port (deliverable increments)
- [ ] P3.1 Inventory/Client state: AgentComponents, AgentSettingItem, CCMEvalGrid, CacheGrid, InstalledSoftwareGrid, ProcessGrid.
- [ ] P3.2 Maintenance/Actions: InstallAgent, InstallRepair, ServicesGrid, ServiceWindowGrid, ServiceWindowNew, PowerSettings.
- [ ] P3.3 Content/updates distribution: AdvertisementGrid, ApplicationGrid, SWUpdates/SWAllUpdates/SWStatus, ExecHistoryGrid.
- [ ] P3.4 Observability: EventMonitoring, LogGrid/LogViewer, WMIBrowser, CollectionVariables.
- [ ] P3.5 Plugins/Customization: decide strategy for Plugins/Customization (WinUI add-ins vs direct migration).
- [ ] P3.6 Targeted tests (interop unit tests + UI smoke) for each migrated slice.

### Phase 4 - Intune addition/alignment
- [ ] P4.1 Choose Graph auth flow (device code or WAM) and store required scopes (DeviceManagement*).
- [ ] P4.2 Implement Intune service (ManagedDevices, Compliance, Apps/Assignments) with pagination and filters.
- [ ] P4.3 SCCM/Intune UX parity: shared views (device details, apps, updates, logs) with source indicator.
- [ ] P4.4 Integration tests on dev tenant (Graph throttling, auth errors, MFA) with consistent error UX.

### Phase 5 - WinUI 3 UX/Navigation
- [ ] P5.1 Adapt CCCM pages into WinUI 3 pages (NavigationView + Frame) following CCEM/DevWinUI styles.
- [ ] P5.2 Add search/title bar and breadcrumbs to the new views, with keyboard commands.
- [ ] P5.3 Light/dark theming and accessibility (contrast, keyboard nav, focus visuals) on migrated modules.

### Phase 6 - Packaging, perf, and final validation
- [ ] P6.1 Verify Velopack/MSIX packaging (icons, AppxManifest, context menu) and native dependencies (WinRM/PS).
- [ ] P6.2 Optimize startup (splash -> interface) and parallel executions (async/await) for remote operations.
- [ ] P6.3 Final test campaign (SCCM + Intune) and author an operations/migration guide.
- [ ] P6.4 Go/No-Go and release channel (Stable/Preview) with changelog.

## Quick checkpoints (major)
- [x] CP0 Initial plan ready.
- [ ] CP1 Full CCCM module/dependency mapping.
- [ ] CP2 WinUI 3 SCCM interop foundation ready (services + tests).
- [ ] CP3 First SCCM batch migrated and navigable in CCEM.
- [ ] CP4 Baseline Intune features delivered.
- [ ] CP5 Release candidate packaged and tested.
