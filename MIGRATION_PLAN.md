# WPF to WinUI 3 Migration Plan - Client Center for Configuration Manager

> **Migration Tracking Document** - Updated for AI-assisted progressive migration

---

## 📊 Migration Progress Tracker

### Overall Progress: 0% Complete

| Phase | Status | Progress | Started | Completed |
|-------|--------|----------|---------|-----------|
| Phase 1: Foundation & Core Library | ⏳ Not Started | 0/4 | - | - |
| Phase 2: Navigation & UI Framework | ⏳ Not Started | 0/3 | - | - |
| Phase 3: Control Migration (29 controls) | ⏳ Not Started | 0/29 | - | - |
| Phase 4: Plugin System (17 plugins) | ⏳ Not Started | 0/17 | - | - |
| Phase 5: Ribbon Actions Migration | ⏳ Not Started | 0/1 | - | - |
| Phase 6: Logging & Diagnostics | ⏳ Not Started | 0/2 | - | - |
| Phase 7: Testing & Quality | ⏳ Not Started | 0/3 | - | - |
| Phase 8: Intune Preparation | ⏳ Not Started | 0/3 | - | - |

**Status Legend:**

- ⏳ Not Started
- 🔄 In Progress
- ✅ Completed
- ⚠️ Blocked
- ❌ Failed (needs rework)

---

## 🎯 Current Sprint Focus

**Sprint**: Sprint 1 - Foundation
**Target Completion**: TBD
**Active Tasks**: Phase 1.1 - Migrate sccmclictr.automation Library

---

## Analysis Summary

**Source**: Client Center for Configuration Manager (WPF, .NET Framework 4.8)

- ~228 source files
- ~18,264 lines in core automation library
- 29 user controls for SCCM management
- 17 extensibility plugins
- PowerShell remoting-based architecture

**Target**: CCEM (WinUI 3, .NET 9, Windows App SDK 1.8)

- Modern MVVM architecture with DevWinUI
- JSON-based navigation system
- Dependency injection with services

**✅ Confirmed Compatibility**:

- **Windows App SDK 1.8** is officially compatible with **.NET 9.0**
- Target Framework Moniker (TFM): `net9.0-windows10.0.26100.0`
- Minimum OS: Windows 10 version 1809 (build 17763)
- Visual Studio 2022 version 17.0+ required

---

## 🔄 WPF to WinUI 3 Conversion Guide

### Critical Conversion Rules

#### ⚠️ IMPORTANT: Every WPF component must be adapted for WinUI 3/.NET 9

### 1. XAML Namespace Changes

**⚠️ IMPORTANT: XAML namespaces are IDENTICAL, but C# types are DIFFERENT!**

**WPF Namespaces** → **WinUI 3 Namespaces**:

```xml
<!-- WPF -->
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- Uses System.Windows.* in code-behind -->
</Window>

<!-- WinUI 3 - SAME XAML namespace! -->
<Window xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        mc:Ignorable="d">
    <!-- Uses Microsoft.UI.Xaml.* in code-behind -->
</Window>
```

**Code-behind - Mandatory Changes**:

```csharp
// ❌ WPF - Namespaces .NET Framework
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

// ✅ WinUI 3 - Namespaces Windows App SDK
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
```

**C# Type Mapping**:

| WPF Type (C#) | WinUI 3 Type (C#) | XAML |
|---------------|-------------------|------|
| `System.Windows.Window` | `Microsoft.UI.Xaml.Window` | `<Window>` (identique) |
| `System.Windows.Controls.Button` | `Microsoft.UI.Xaml.Controls.Button` | `<Button>` (identique) |
| `System.Windows.Controls.Grid` | `Microsoft.UI.Xaml.Controls.Grid` | `<Grid>` (identique) |
| `System.Windows.Application` | `Microsoft.UI.Xaml.Application` | `<Application>` (identique) |

### 2. Control Mapping Reference

| WPF Control | WinUI 3 Equivalent | Migration Notes |
|-------------|-------------------|-----------------|
| `System.Windows.Controls.DataGrid` | `Microsoft.UI.Xaml.Controls.DataGrid` | ⚠️ Use CommunityToolkit.WinUI.UI.Controls.DataGrid |
| `System.Windows.Controls.Ribbon` | `CommandBar` + `MenuBar` | 🔨 Complete redesign needed |
| `System.Windows.Controls.RichTextBox` | `RichEditBox` | ⚠️ Different API, content model changed |
| `System.Windows.Controls.TreeView` | `TreeView` | ⚠️ ItemTemplate structure different |
| `Toolkit.AutoCompleteBox` | `AutoSuggestBox` | ✅ Similar API |
| `System.Windows.Controls.UserControl` | `UserControl` or `Page` | ✅ Direct conversion |
| `System.Windows.Window` | `Microsoft.UI.Xaml.Window` | ⚠️ Different lifecycle |
| `System.Windows.Controls.PasswordBox` | `PasswordBox` | ✅ Direct conversion |
| `System.Windows.Controls.TextBlock` | `TextBlock` | ✅ Direct conversion |
| `System.Windows.Controls.Button` | `Button` | ✅ Direct conversion |
| `System.Windows.Controls.Grid` | `Grid` | ✅ Direct conversion |
| `System.Windows.Controls.StackPanel` | `StackPanel` | ✅ Direct conversion |

### 2.1 WPF Controls NOT Supported in WinUI 3

#### ⚠️ IMPORTANT: The following controls have NO direct equivalent in WinUI 3

| WPF Control | WinUI 3 Status | Recommended Alternative | Notes |
|--------------|----------------|-------------------------|-------|
| `InkCanvas` | ❌ **Not supported** (v1.7+) | [Win2D for WinUI 3](https://microsoft.github.io/Win2D/WinUI3/html/Introduction.htm) | Requires custom implementation with Win2D |
| `MediaElement` | ❌ Deprecated | `MediaPlayerElement` | Available since Windows App SDK 1.2 |
| `WebBrowser` | ❌ Not supported | `WebView2` | Mandatory migration to Chromium |
| `WindowsFormsHost` | ❌ Not supported | None (complete migration required) | No WinForms interop in WinUI 3 |
| `FlowDocument` | ❌ Not supported | `RichEditBox` with limitations | Reduced functionality |

**Actions for this project**:

- ⚠️ **ScheduleControl** (lines 510-514) potentially uses Canvas - verify if InkCanvas is used
- ✅ If yes: migrate to Win2D or design an alternative with `Microsoft.UI.Composition`

### 3. Code-Behind Conversion Patterns

#### Pattern 1: Event Handlers

```csharp
// ❌ WPF Pattern
private void Button_Click(object sender, RoutedEventArgs e)
{
    // Direct UI manipulation
    textBox.Text = "Updated";
}

// ✅ WinUI 3 + MVVM Pattern
// In ViewModel:
[RelayCommand]
private void UpdateText()
{
    Text = "Updated";
}

// In XAML:
<Button Command="{Binding UpdateTextCommand}" />
```

#### Pattern 2: Data Binding

```csharp
// ❌ WPF INotifyPropertyChanged
private string _text;
public string Text
{
    get => _text;
    set
    {
        _text = value;
        OnPropertyChanged(nameof(Text));
    }
}

// ✅ WinUI 3 ObservableProperty (CommunityToolkit.Mvvm)
[ObservableProperty]
private string text;
```

#### ⚠️ Important Note on AOT Warnings

Using `[ObservableProperty]` on **private fields** (pattern above) generates warnings **MVVMTK0045** and **MVVMTK0051** in WinRT scenarios with NativeAOT.

##### Option 1: Use current pattern (recommended for this project)

```csharp
// ✅ Simple pattern - Generates AOT warnings (ignorable if not using NativeAOT)
[ObservableProperty]
private string text;
```

- ✅ Simple and concise
- ✅ Works perfectly in standard mode
- ⚠️ Generates warnings if NativeAOT is enabled (not used in this project)

##### Option 2: AOT-compatible pattern (optional)

```csharp
// ✅ AOT-compatible pattern - No warnings (C# 11+, VS 17.12+)
[ObservableProperty]
public partial string Text { get; set; }
```

- ✅ NativeAOT compatible
- ✅ No warnings
- ⚠️ Requires C# 11+ and partial properties

**Decision for this project**: Use **Option 1** (private fields) since NativeAOT is not required.

#### Pattern 3: Collections

```csharp
// ❌ WPF Code-behind
dataGrid.ItemsSource = GetItems();

// ✅ WinUI 3 ViewModel
[ObservableProperty]
private ObservableCollection<Item> items;

public async Task LoadItemsAsync()
{
    var data = await _service.GetItemsAsync();
    Items = new ObservableCollection<Item>(data);
}
```

#### Pattern 4: Dependency Properties

```csharp
// ❌ WPF Dependency Property
public static readonly DependencyProperty TextProperty =
    DependencyProperty.Register("Text", typeof(string), typeof(MyControl));

// ✅ WinUI 3 - Same syntax but different namespace
using Microsoft.UI.Xaml; // Not System.Windows!

public static readonly DependencyProperty TextProperty =
    DependencyProperty.Register("Text", typeof(string), typeof(MyControl),
    new PropertyMetadata(default(string)));
```

### 4. Threading & Dispatcher

```csharp
// ❌ WPF Dispatcher
Application.Current.Dispatcher.Invoke(() =>
{
    textBlock.Text = "Updated";
});

// ✅ WinUI 3 DispatcherQueue
DispatcherQueue.TryEnqueue(() =>
{
    textBlock.Text = "Updated";
});

// OR use ViewModel (preferred)
// No dispatcher needed with proper MVVM
```

### 5. Resource Dictionaries

```xml
<!-- ❌ WPF -->
<ResourceDictionary Source="/Themes/Generic.xaml"/>

<!-- ✅ WinUI 3 -->
<ResourceDictionary Source="ms-appx:///Themes/Generic.xaml"/>
```

### 6. Common WPF Features NOT in WinUI 3

| WPF Feature | WinUI 3 Alternative |
|-------------|---------------------|
| `Ribbon` control | `CommandBar` + `MenuBar` + custom layout |
| `RoutedCommand` | `ICommand` (RelayCommand from CommunityToolkit) |
| `Triggers` (EventTrigger, DataTrigger) | `Behaviors` from Microsoft.Xaml.Behaviors.WinUI |
| `VisualStateManager` triggers | `VisualStateManager` (different syntax) |
| `NavigationWindow` | `Frame` with custom navigation service |
| `WebBrowser` control | `WebView2` |

### 7. Migration Checklist for Each Control

For each WPF control being migrated, verify:

- [ ] ✅ **Namespaces updated** to WinUI 3 equivalents
- [ ] ✅ **XAML syntax** compatible (x:Class, DataContext, etc.)
- [ ] ✅ **Controls replaced** with WinUI 3 equivalents
- [ ] ✅ **Event handlers** moved to ViewModel commands
- [ ] ✅ **Data binding** uses ObservableProperty
- [ ] ✅ **Collections** use ObservableCollection
- [ ] ✅ **No code-behind** business logic (MVVM)
- [ ] ✅ **Resources** use ms-appx:/// URIs
- [ ] ✅ **Styles and templates** converted
- [ ] ✅ **Converters** migrated if needed
- [ ] ✅ **ViewModel registered** in DI container
- [ ] ✅ **Navigation** added to AppData.json
- [ ] ✅ **Tested** with WinUI 3 runtime

### 8. PowerShell Integration Notes

```csharp
// ❌ WPF with .NET Framework 4.8
using System.Management.Automation;
using System.Management.Automation.Runspaces;

// ✅ WinUI 3 with .NET 9
// SAME - but ensure you use PowerShell 7+ SDK
using System.Management.Automation;
using System.Management.Automation.Runspaces;

// NuGet: Microsoft.PowerShell.SDK (for .NET 9)
```

### 9. Common Migration Pitfalls

| Issue | WPF Behavior | WinUI 3 Behavior | Solution |
|-------|--------------|------------------|----------|
| **Window.Owner** | Supported | ❌ Not supported | Use `XamlRoot` for dialogs |
| **MessageBox** | `MessageBox.Show()` | ❌ Not available | Use `ContentDialog` |
| **FileDialog** | `Microsoft.Win32.OpenFileDialog` | ❌ Different | Use `Windows.Storage.Pickers.FileOpenPicker` |
| **Clipboard** | `Clipboard.SetText()` | Different namespace | Use `Windows.ApplicationModel.DataTransfer.Clipboard` |
| **App.xaml.cs** | Inherits `Application` | Different initialization | Override `OnLaunched` instead of `StartupUri` |

#### ⚠️ ContentDialog and XamlRoot - MANDATORY

#### WPF → WinUI 3: XamlRoot is REQUIRED for dialogs

```csharp
// ❌ WPF - MessageBox.Show()
MessageBoxResult result = MessageBox.Show(
    "Are you sure?",
    "Confirmation",
    MessageBoxButton.YesNo,
    MessageBoxImage.Question
);

if (result == MessageBoxResult.Yes)
{
    // Action confirmed
}

// ✅ WinUI 3 - ContentDialog with XamlRoot MANDATORY
var dialog = new ContentDialog
{
    Title = "Confirmation",
    Content = "Are you sure?",
    PrimaryButtonText = "Yes",
    CloseButtonText = "No",
    DefaultButton = ContentDialogButton.Primary,

    // ⚠️ WITHOUT XamlRoot → GUARANTEED CRASH!
    XamlRoot = this.Content.XamlRoot  // ← MANDATORY from Page/UserControl
};

ContentDialogResult result = await dialog.ShowAsync();

if (result == ContentDialogResult.Primary)
{
    // Action confirmed
}
```

**Getting XamlRoot depending on context**:

```csharp
// From a Page
XamlRoot = this.XamlRoot;

// From a UserControl
XamlRoot = this.XamlRoot;

// From a ViewModel (via injection)
XamlRoot = App.GetService<MainWindow>().Content.XamlRoot;

// From MainWindow.xaml.cs
XamlRoot = this.Content.XamlRoot;
```

**Recommended pattern for ViewModels**:

```csharp
// Dialog helper service
public interface IDialogService
{
    Task<ContentDialogResult> ShowConfirmationAsync(string title, string content);
}

public class DialogService : IDialogService
{
    private XamlRoot _xamlRoot;

    public DialogService(MainWindow mainWindow)
    {
        _xamlRoot = mainWindow.Content.XamlRoot;
    }

    public async Task<ContentDialogResult> ShowConfirmationAsync(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = "Yes",
            CloseButtonText = "No",
            XamlRoot = _xamlRoot  // Automatically injected
        };

        return await dialog.ShowAsync();
    }
}
```

---

## Architecture Decision

### Namespace & Project Structure

```text
src/CCEM/
├── SCCM/
│   ├── Automation/              # Migrated sccmclictr.automation library
│   │   ├── SCCMAgent.cs
│   │   ├── AgentActions.cs
│   │   ├── Components.cs
│   │   └── ... (all automation classes)
│   ├── Services/
│   │   ├── SCCMConnectionService.cs
│   │   ├── PluginLoaderService.cs
│   │   └── Interfaces/
│   ├── Models/                  # SCCM-specific data models
│   ├── ViewModels/              # SCCM ViewModels
│   └── Plugins/                 # SCCM plugin infrastructure
├── Intune/                      # Future Intune support
│   ├── Graph/                   # Microsoft Graph API integration
│   ├── Services/
│   ├── Models/
│   └── ViewModels/
├── Shared/                      # Common utilities & services
│   ├── Services/
│   ├── Models/
│   ├── Helpers/
│   └── Converters/
├── Views/
│   ├── SCCM/                    # All SCCM pages
│   └── Intune/                  # Future Intune pages
├── ViewModels/                  # Shared ViewModels
├── Common/                      # App-level configuration
└── ... (existing structure)
```

**Primary Namespace**: `CCEM.SCCM.Automation`

- Future-ready for `CCEM.Intune.*` namespaces
- Clear separation of concerns
- Scalable architecture

---

## Phase 1: Foundation & Core Library Migration

### Status: ⏳ Not Started (0/4 tasks complete)

### 1.1 Migrate sccmclictr.automation Library

**Status**: ⏳ Not Started
**Target**: `src/CCEM/SCCM/Automation/`

**Tasks**:

1. [ ] Create new folder structure `SCCM/Automation/`
2. [ ] Copy all .cs files from `sccmclictr/sccmclictrlib/sccmclictr.automation/`
3. [ ] Update namespace from `sccmclictr.automation` to `CCEM.SCCM.Automation`
4. [ ] Update namespace in sub-namespaces:
   - [ ] `sccmclictr.automation.functions` → `CCEM.SCCM.Automation.Functions`
5. [ ] Port to .NET 9 (`net9.0-windows10.0.26100.0`)
6. [ ] Update dependencies:
   - [ ] ✅ Keep: `System.Management.Automation` (PowerShell 7+)
   - [ ] ✅ Keep: `System.Management` (WMI)
   - [ ] ❌ Remove: .NET Framework-specific references
7. [ ] **WPF Conversion**: Remove any WPF-specific code
8. [ ] Test all core classes:
   - [ ] `SCCMAgent` (connection & runspace management)
   - [ ] `AgentActions` (inventory, policy, etc.)
   - [ ] `Components`, `Policy`, `SoftwareDistribution`, etc.

**Files to migrate** (~20 core classes):

- [ ] SCCMAgent.cs
- [ ] AgentActions.cs
- [ ] AgentProperties.cs
- [ ] SoftwareDistribution.cs
- [ ] Components.cs
- [ ] Policy.cs
- [ ] Inventory.cs
- [ ] SoftwareUpdates.cs
- [ ] Health.cs
- [ ] Services.cs
- [ ] Monitoring.cs
- [ ] LocationServices.cs
- [ ] Processes.cs
- [ ] SWCache.cs
- [ ] DCM.cs
- [ ] AppV.cs
- [ ] WSMan.cs
- [ ] ScheduleDecoding.cs
- [ ] BaseInit.cs
- [ ] Common.cs

**WPF Conversion Notes**:

- ⚠️ These are pure C# classes with PowerShell - no WPF dependencies expected
- ✅ Should port cleanly to .NET 9
- ⚠️ Check for any `System.Windows` references and remove

### 1.2 Create SCCM Services Layer

**Status**: ⏳ Not Started
**Target**: `src/CCEM/SCCM/Services/`

**New Services**:

1. [ ] **ISCCMConnectionService** - Manages SCCMAgent lifecycle
2. [ ] **ISCCMPluginService** - Handles plugin discovery and loading
3. [ ] **ISCCMDataService** - Provides data access for SCCM entities
4. [ ] Register all services in `App.xaml.cs` → `ConfigureServices()`

### 1.3 Create SCCM Models

**Status**: ⏳ Not Started
**Target**: `src/CCEM/SCCM/Models/`

**Create wrapper models** for automation library entities:

- [ ] `ComponentModel.cs` - wraps automation Components
- [ ] `ApplicationModel.cs` - wraps automation Applications
- [ ] `UpdateModel.cs` - wraps automation Software Updates
- [ ] `ServiceModel.cs` - wraps automation Services
- [ ] `PolicyModel.cs` - wraps automation Policy

**WPF Conversion**:

- ✅ Use `ObservableObject` base class (CommunityToolkit.Mvvm)
- ✅ Use `[ObservableProperty]` attributes
- ❌ No `INotifyPropertyChanged` manual implementation

### 1.4 Create Shared Utilities

**Status**: ⏳ Not Started
**Target**: `src/CCEM/Shared/`

**Common Services**:

- [ ] `Shared/Helpers/CommandHelper.cs` - common commands
- [ ] `Shared/Converters/` - value converters

---

## Phase 2: Navigation & UI Framework

### Status: ⏳ Not Started (0/3 tasks complete)

### 2.1 Update Navigation Structure

**Status**: ⏳ Not Started
**File**: `Assets/NavViewMenu/AppData.json`

- [ ] Add SCCM section with navigation items
- [ ] Add Intune placeholder section
- [ ] Update existing Settings section
- [ ] T4 templates will auto-generate mappings

### 2.2 Create Connection UI

**Status**: ⏳ Not Started
**New File**: `Views/SCCM/ConnectionPage.xaml`

- [ ] Create XAML page with WinUI 3 controls
- [ ] ✅ Use `AutoSuggestBox` (not WPF AutoCompleteBox)
- [ ] ✅ Use `Button` for Connect/Disconnect
- [ ] ✅ Use `CommandBar` for connection actions
- [ ] ✅ Use `TeachingTip` for connection options
- [ ] Create `ConnectionViewModel.cs`
- [ ] Register ViewModel in DI

**WPF Conversion**:

- ❌ Remove Ribbon connection panel
- ✅ Replace with modern WinUI 3 CommandBar
- ✅ `AutoCompleteBox` → `AutoSuggestBox`

### 2.3 Main Window Updates

**Status**: ⏳ Not Started
**File**: `MainWindow.xaml`

- [ ] Keep existing DevWinUI `NavigationView`
- [ ] Add `CommandBar` for global actions
- [ ] Add connection status indicator
- [ ] Add plugin menu items dynamically

---

## Phase 3: Control Migration (29 Controls)

### Status: ⏳ Not Started (0/29 controls complete)

### Migration Pattern Template

**For each control, follow this WPF → WinUI 3 conversion pattern:**

1. [ ] **Create XAML Page** (`Views/SCCM/[Name]Page.xaml`)
   - ✅ Update namespaces to WinUI 3
   - ✅ Replace WPF controls with WinUI 3 equivalents
   - ✅ Convert Ribbon actions to CommandBar
   - ✅ Use `ms-appx:///` for resources

2. [ ] **Create ViewModel** (`ViewModels/SCCM/[Name]ViewModel.cs`)
   - ✅ Inherit from `ObservableObject`
   - ✅ Use `[ObservableProperty]` for properties
   - ✅ Use `[RelayCommand]` for commands
   - ❌ No code-behind business logic

3. [ ] **Register ViewModel** in `App.xaml.cs`
   - ✅ Add to `ConfigureServices()`

4. [ ] **Add to Navigation** in `AppData.json`

5. [ ] **Test** with WinUI 3 runtime

### 3.1 Core Controls (Weeks 1-2)

**Status**: ⏳ Not Started (0/4 complete)

| # | Control | Source | Target | Status | Notes |
|---|---------|--------|--------|--------|-------|
| 1 | Connection | New | `ConnectionPage.xaml` | ⏳ | AutoCompleteBox → AutoSuggestBox |
| 2 | Components | `AgentComponents.xaml` | `ComponentsPage.xaml` | ⏳ | DataGrid conversion needed |
| 3 | Settings | `AgentSettingItem.xaml` | `SettingsPage.xaml` | ⏳ | Form controls |
| 4 | Install/Repair | `InstallRepair.xaml` | `InstallRepairPage.xaml` | ⏳ | Action buttons |

### 3.2 Inventory Controls (Weeks 3-4)

**Status**: ⏳ Not Started (0/5 complete)

| # | Control | Source | Target | Status | Notes |
|---|---------|--------|--------|--------|-------|
| 5 | Cache | `CacheGrid.xaml` | `CachePage.xaml` | ⏳ | DataGrid with actions |
| 6 | Applications | `ApplicationGrid.xaml` | `ApplicationsPage.xaml` | ⏳ | DataGrid + commands |
| 7 | Installed Software | `InstalledSoftwareGrid.xaml` | `InstalledSoftwarePage.xaml` | ⏳ | Export functionality |
| 8 | Advertisements | `AdvertisementGrid.xaml` | `AdvertisementsPage.xaml` | ⏳ | Execution actions |
| 9 | Exec History | `ExecHistoryGrid.xaml` | `ExecutionHistoryPage.xaml` | ⏳ | History grid |

### 3.3 Software Updates (Week 5)

**Status**: ⏳ Not Started (0/3 complete)

| # | Control | Source | Target | Status | Notes |
|---|---------|--------|--------|--------|-------|
| 10 | Updates | `SWUpdatesGrid.xaml` | `UpdatesPage.xaml` | ⏳ | Install actions |
| 11 | All Updates | `SWAllUpdatesGrid.xaml` | `AllUpdatesPage.xaml` | ⏳ | Compliance info |
| 12 | Update Status | `SWStatusGrid.xaml` | `UpdateStatusPage.xaml` | ⏳ | Status display |

### 3.4 System Controls (Weeks 6-7)

**Status**: ⏳ Not Started (0/5 complete)

| # | Control | Source | Target | Status | Notes |
|---|---------|--------|--------|--------|-------|
| 13 | Services | `ServicesGrid.xaml` | `ServicesPage.xaml` | ⏳ | Start/Stop actions |
| 14 | Processes | `ProcessGrid.xaml` | `ProcessesPage.xaml` | ⏳ | Kill process |
| 15 | Log Viewer | `LogViewer.xaml` | `LogViewerPage.xaml` | ⏳ | RichTextBox → RichEditBox |
| 16 | Logs | `LogGrid.xaml` | `LogsPage.xaml` | ⏳ | File selection |
| 17 | Event Monitoring | `EventMonitoring.xaml` | `EventMonitoringPage.xaml` | ⏳ | Real-time events |

### 3.5 Advanced Controls (Weeks 8-9)

**Status**: ⏳ Not Started (0/9 complete)

| # | Control | Source | Target | Status | Notes |
|---|---------|--------|--------|--------|-------|
| 18 | Service Windows | `ServiceWindowGrid.xaml` | `ServiceWindowsPage.xaml` | ⏳ | Schedule viz |
| 19 | Edit Service Window | `ServiceWindowNew.xaml` | `ServiceWindowEditPage.xaml` | ⏳ | Calendar control |
| 20 | Variables | `CollectionVariables.xaml` | `VariablesPage.xaml` | ⏳ | CRUD operations |
| 21 | Power | `PowerSettings.xaml` | `PowerPage.xaml` | ⏳ | Power management |
| 22 | WMI Browser | `WMIBrowser.xaml` | `WMIBrowserPage.xaml` | ⏳ | TreeView conversion |
| 23 | Evaluation | `CCMEvalGrid.xaml` | `EvaluationPage.xaml` | ⏳ | Rules display |
| 24 | Install Agent | `InstallAgent.xaml` | `InstallAgentPage.xaml` | ⏳ | Wizard UI |
| 25 | Import App | `ImportApp.xaml` | `ImportAppPage.xaml` | ⏳ | Import wizard |
| 26 | About | `About.xaml` | `AboutPage.xaml` | ⏳ | ContentDialog |

### 3.6 Special Controls

**Status**: ⏳ Not Started (0/1 complete)

| # | Control | Source | Target | Status | Notes |
|---|---------|--------|--------|--------|-------|
| 27 | Schedule Control | `ScheduleControl/*.xaml` | Custom UserControl | ⏳ | Win2D/Composition |

**WPF Conversion for Special Controls**:

- ⚠️ `ScheduleControl` uses WPF Canvas - need to redesign with Win2D or Composition API
- ⚠️ Timeline visualization requires custom rendering

---

## Phase 4: Plugin System Migration

### Status: ⏳ Not Started (0/17 plugins complete)

### 4.1 Plugin Architecture

**Status**: ⏳ Not Started
**Target**: `src/CCEM/SCCM/Plugins/`

- [ ] Create `IPlugin` interface for WinUI 3
- [ ] Create `IAgentActionPlugin` (replaces AgentActionTool_)
- [ ] Create `ICustomToolPlugin` (replaces CustomTools_)
- [ ] Create `IMainMenuPlugin` (replaces MainMenu_)
- [ ] Implement plugin discovery service
- [ ] Create plugin loading mechanism

**WPF Conversion**:

- ❌ Remove reflection-based WPF UI extraction
- ✅ Use proper WinUI 3 UIElement composition
- ✅ Update namespaces from `System.Windows` to `Microsoft.UI.Xaml`

### 4.2 Migrate Priority Plugins (Weeks 10-12)

**Status**: ⏳ Not Started (0/17 complete)

| # | Plugin | Source | Target | Status | Priority | WPF Notes |
|---|--------|--------|--------|--------|----------|-----------|
| 1 | RDP | `Plugin_RDP` | `CCEM.SCCM.Plugins.RDP` | ⏳ | High | Process launch - no WPF |
| 2 | CompMgmt | `Plugin_CompMgmt` | `CCEM.SCCM.Plugins.ComputerManagement` | ⏳ | High | Process launch |
| 3 | RemoteTools | `Plugin_RemoteTools` | `CCEM.SCCM.Plugins.RemoteTools` | ⏳ | High | Integration tool |
| 4 | ResourceExplorer | `Plugin_ResourceExplorer` | `CCEM.SCCM.Plugins.ResourceExplorer` | ⏳ | High | Launcher |
| 5 | StatusMessageViewer | `Plugin_StatusMessageViewer` | `CCEM.SCCM.Plugins.StatusMessageViewer` | ⏳ | High | Grid control |
| 6 | Explorer | `Plugin_Explorer` | `CCEM.SCCM.Plugins.FileExplorer` | ⏳ | Medium | Process launch |
| 7 | Regedit | `Plugin_Regedit` | `CCEM.SCCM.Plugins.RegistryEditor` | ⏳ | Medium | Process launch |
| 8 | MSInfo32 | `Plugin_MSInfo32` | `CCEM.SCCM.Plugins.SystemInfo` | ⏳ | Medium | Process launch |
| 9 | MSRA | `Plugin_MSRA` | `CCEM.SCCM.Plugins.RemoteAssistance` | ⏳ | Medium | Process launch |
| 10 | PSScripts | `Plugin_PSScripts` | `CCEM.SCCM.Plugins.PowerShellScripts` | ⏳ | Medium | UI conversion |
| 11 | FEP | `Plugin_FEP` | `CCEM.SCCM.Plugins.EndpointProtection` | ⏳ | Low | Actions |
| 12 | AppV | `Plugin_AppV46` | `CCEM.SCCM.Plugins.AppV` | ⏳ | Low | UI conversion |
| 13 | EnablePSRemoting | `Plugin_EnablePSRemoting` | `CCEM.SCCM.Plugins.EnablePSRemoting` | ⏳ | Low | Action button |
| 14 | RuckZuck | `Plugin_RuckZuck` | `CCEM.SCCM.Plugins.RuckZuck` | ⏳ | Low | UI conversion |
| 15 | AMTTools | `Plugin_CustomTools_AMTTools` | `CCEM.SCCM.Plugins.AMTTools` | ⏳ | Low | Process launch |
| 16 | SelfUpdate | `Plugin_SelfUpdate` | Built-in mechanism | ⏳ | Low | Remove plugin |
| 17 | Customization | `Customization` | Evaluate | ⏳ | Low | Licensing |

**Plugin Migration Pattern**:
For each plugin:

1. [ ] Copy source files
2. [ ] ✅ Convert XAML to WinUI 3 (namespaces, controls)
3. [ ] ✅ Update code-behind to use WinUI 3 types
4. [ ] ✅ Remove WPF-specific reflection/UI extraction
5. [ ] ✅ Implement IPlugin interface
6. [ ] Register with plugin service
7. [ ] Test loading and execution

### 4.3 Plugin Integration UI

**Status**: ⏳ Not Started

- [ ] Add plugin buttons to CommandBar
- [ ] Dynamic menu generation
- [ ] Plugin lifecycle management

---

## Phase 5: Ribbon Actions Migration

### Status: ⏳ Not Started (0/1 complete)

### 5.1 Agent Actions

**Status**: ⏳ Not Started

**WPF Ribbon → WinUI 3 CommandBar Conversion**:

- [ ] ❌ Remove WPF Ribbon control
- [ ] ✅ Create CommandBar with MenuFlyouts
- [ ] Inventory Group (HW Inv, SW Inv, DDR)
- [ ] Policy Group (Machine/User Policy)
- [ ] Software Updates Group (Scan, Evaluate)
- [ ] Create ViewModel commands for all actions

**WPF Conversion**:

- ❌ `RibbonButton` → ✅ `AppBarButton`
- ❌ `RibbonSplitButton` → ✅ `AppBarButton` with `MenuFlyout`
- ❌ `RibbonGroup` → ✅ `CommandBar` sections

---

## Phase 6: Logging & Diagnostics

### Status: ⏳ Not Started (0/2 complete)

### 6.1 Trace Listener Integration

**Status**: ⏳ Not Started

- [ ] Integrate existing `MyTraceListener` with Serilog
- [ ] Create `LogViewerPage` with WinUI 3 controls
- [ ] ✅ RichTextBox → RichEditBox for log display
- [ ] Real-time log streaming
- [ ] PowerShell command tracing

**WPF Conversion**:

- ❌ `RichTextBox` → ✅ `RichEditBox`
- ✅ Use `ObservableCollection<LogEntry>` for binding

### 6.2 Developer Mode

**Status**: ⏳ Not Started

- [ ] Enable in AppConfig
- [ ] Show PowerShell commands
- [ ] Detailed logging
- [ ] Performance metrics

---

## Phase 7: Testing & Quality

### Status: ⏳ Not Started (0/3 complete)

### 7.1 Unit Testing

**Status**: ⏳ Not Started
**Create**: `tests/CCEM.Tests`

- [ ] Test SCCM.Automation library
- [ ] Test Services
- [ ] Test ViewModels

### 7.2 Integration Testing

**Status**: ⏳ Not Started

- [ ] Test with real SCCM client
- [ ] Test remote connections
- [ ] Test all agent actions
- [ ] Plugin loading/unloading

### 7.3 UI Testing

**Status**: ⏳ Not Started

- [ ] WinAppDriver setup
- [ ] Critical workflow tests

---

## Phase 8: Intune Preparation

**Progress:** ⏳ Not Started (0/3 complete)

### 8.1 Architecture Foundation

**Status**: ⏳ Not Started
**Target**: `src/CCEM/Intune/`

- [ ] Create folder structure
- [ ] Plan Microsoft Graph integration
- [ ] Design service architecture

### 8.2 Placeholder UI

**Status**: ⏳ Not Started

- [ ] Create `ComingSoonPage.xaml`
- [ ] Add to navigation

### 8.3 Mode Switcher

**Status**: ⏳ Not Started

- [ ] Toggle between SCCM/Intune
- [ ] Save preference

---

## Quick Reference: Next Steps

**When starting work, always**:

1. ✅ Check Migration Progress Tracker for current phase
2. ✅ Review WPF to WinUI 3 Conversion Guide
3. ✅ Follow the migration checklist for each control
4. ✅ Update status in this document after completing tasks
5. ✅ Test thoroughly with WinUI 3 runtime

**Status Update Format**:

```text
[Component Name]
Status: ⏳ → 🔄 (In Progress)
Started: [Date]
Notes: [Any WPF conversion issues encountered]

... (work completed) ...

Status: 🔄 → ✅ (Completed)
Completed: [Date]
Notes: [Final notes, known issues]
```

---

## End of Migration Plan
