# WPF to WinUI 3 Migration Plan - Client Center for Configuration Manager

> **Migration Tracking Document** - Updated for AI-assisted progressive migration

---

## 📊 Migration Progress Tracker

### Overall Progress: 25% Complete (2/8 phases)

| Phase | Status | Progress | Started | Completed |
|-------|--------|----------|---------|-----------|
| Phase 1: Foundation & Core Library | ✅ Completed | 4/4 | 2025-10-04 | 2025-10-04 |
| Phase 2: Navigation & UI Framework | ✅ Completed | 3/3 | 2025-10-05 | 2025-10-05 |
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

**Sprint**: Sprint 3 - Control Migration (Phase 3)
**Target Completion**: TBD
**Active Tasks**: Ready to start Phase 3 - Control Migration

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

### Status: ✅ Completed (4/4 tasks complete) - Completed: 2025-10-04

### 1.1 Migrate sccmclictr.automation Library

**Status**: ✅ Completed
**Target**: `src/CCEM/SCCM/Automation/`

**Tasks**:

1. [x] Create new folder structure `SCCM/Automation/`
2. [x] Copy all .cs files from `sccmclictr/sccmclictrlib/sccmclictr.automation/`
3. [x] Update namespace from `sccmclictr.automation` to `CCEM.SCCM.Automation`
4. [x] Update namespace in sub-namespaces:
   - [x] `sccmclictr.automation.functions` → `CCEM.SCCM.Automation.Functions`
5. [x] Port to .NET 9 (`net9.0-windows10.0.26100.0`)
6. [x] Update dependencies:
   - [x] ✅ Keep: `System.Management.Automation` (PowerShell 7+)
   - [x] ✅ Keep: `System.Management` (WMI)
   - [x] ❌ Remove: .NET Framework-specific references
7. [x] **WPF Conversion**: Remove any WPF-specific code
8. [x] Test all core classes:
   - [x] `SCCMAgent` (connection & runspace management)
   - [x] `AgentActions` (inventory, policy, etc.)
   - [x] `Components`, `Policy`, `SoftwareDistribution`, etc.

**Files migrated** (23 core classes):

- [x] SCCMAgent.cs
- [x] AgentActions.cs
- [x] AgentProperties.cs
- [x] SoftwareDistribution.cs
- [x] Components.cs
- [x] Policy.cs
- [x] Inventory.cs
- [x] SoftwareUpdates.cs
- [x] Health.cs
- [x] Services.cs
- [x] Monitoring.cs
- [x] LocationServices.cs
- [x] Processes.cs
- [x] SWCache.cs
- [x] DCM.cs
- [x] AppV.cs
- [x] WSMan.cs
- [x] ScheduleDecoding.cs
- [x] BaseInit.cs
- [x] Common.cs
- [x] DDRGen.cs
- [x] Properties/Resources.Designer.cs
- [x] Properties/Settings.Designer.cs

**Build Fixes Applied**:
- Fixed namespace reference in AgentActions.cs (sccmclictr → CCEM.SCCM.Automation)
- Added missing Settings.Designer.cs and Settings.settings files
- Fixed image type ambiguity in Common.cs (System.Drawing.Image vs Microsoft.UI.Xaml.Controls.Image)
- Project builds successfully with expected nullable warnings

**WPF Conversion Notes**:

- ✅ No WPF dependencies found - pure C# classes with PowerShell
- ✅ Ported cleanly to .NET 9
- ✅ System.Drawing.Image types properly qualified to avoid WinUI conflicts

### 1.2 Create SCCM Services Layer

**Status**: ✅ Completed
**Target**: `src/CCEM/SCCM/Services/`

**Services Created**:

1. [x] **ISCCMConnectionService** / **SCCMConnectionService** - Manages SCCMAgent lifecycle
2. [x] **ISCCMPluginService** / **SCCMPluginService** - Handles plugin discovery and loading
3. [x] **ISCCMDataService** / **SCCMDataService** - Provides data access for SCCM entities
4. [x] Register all services in `App.xaml.cs` → `ConfigureServices()`

**Build Fixes Applied**:
- Updated SCCMConnectionService to use proper SCCMAgent constructors (no parameterless constructor)
- Fixed SCCMDataService property access (oComponents → Client.Components)
- All services properly registered as Singletons in DI container

### 1.3 Create SCCM Models

**Status**: ✅ Completed
**Target**: `src/CCEM/SCCM/Models/`

**Models Created**:

- [x] `ComponentModel.cs` - wraps automation Components
- [x] `ApplicationModel.cs` - wraps automation Applications
- [x] `UpdateModel.cs` - wraps automation Software Updates
- [x] `ServiceModel.cs` - wraps automation Services
- [x] `PolicyModel.cs` - wraps automation Policy

**WPF Conversion**:

- ✅ All models use `ObservableObject` base class (CommunityToolkit.Mvvm)
- ✅ All properties use `[ObservableProperty]` attributes
- ✅ No manual `INotifyPropertyChanged` implementation needed

### 1.4 Create Shared Utilities

**Status**: ✅ Completed
**Target**: `src/CCEM/Shared/`

**Utilities Created**:

- [x] `Shared/Helpers/CommandHelper.cs` - async command execution, PowerShell support, UAC elevation
- [x] `Shared/Converters/` - folder created for Phase 2

---

## Phase 2: Navigation & UI Framework

### Status: ✅ Completed (3/3 tasks complete) - Completed: 2025-10-05

### 2.1 Update Navigation Structure

**Status**: ✅ Completed
**File**: `Assets/NavViewMenu/AppData.json`

- [x] Add SCCM section with navigation items
- [x] Add Intune placeholder section
- [x] Update existing Settings section
- [x] T4 templates will auto-generate mappings

---

## 🎨 Navigation Menu Structure

### ⚖️ Decision Criteria

Consider these factors when choosing:
- **User workflow** - How do admins typically work?
- **Scalability** - Easy to add Intune features later?
- **Discoverability** - Can users find features easily?
- **Visual balance** - Not too cluttered?

---

### 📐 Platform-First Navigation (Selected)

**Concept**: Top-level split between SCCM and Intune, each with their own sub-structure

**Pros**:
- ✅ Clear separation of concerns
- ✅ Easy to add Intune features incrementally
- ✅ Users immediately know which platform they're managing
- ✅ Best for orgs managing both SCCM and Intune

**Cons**:
- ⚠️ More clicks to reach specific features
- ⚠️ Some duplicate categories (e.g., both have "Applications")

```json
{
  "Groups": [
    {
      "Id": "HomeGroup",
      "Title": "Home",
      "Glyph": "\uE80F",
      "Items": [
        {
          "Id": "Dashboard",
          "Title": "Dashboard",
          "Glyph": "\uE8BC",
          "PageType": "CCEM.Views.DashboardPage",
          "Description": "Unified overview of SCCM and Intune"
        },
        {
          "Id": "QuickActions",
          "Title": "Quick Actions",
          "Glyph": "\uE945",
          "PageType": "CCEM.Views.QuickActionsPage"
        }
      ]
    },
    {
      "Id": "SCCMGroup",
      "Title": "Configuration Manager",
      "Glyph": "\uE950",
      "IsExpandable": true,
      "Items": [
        {
          "Id": "SCCMConnection",
          "Title": "Connection",
          "Glyph": "\uE703",
          "PageType": "CCEM.Views.SCCM.ConnectionPage"
        },
        {
          "Id": "SCCMAgent",
          "Title": "Agent Management",
          "Glyph": "\uE713",
          "HasSubItems": true,
          "SubItems": [
            { "Id": "Components", "Title": "Components", "PageType": "CCEM.Views.SCCM.ComponentsPage" },
            { "Id": "Settings", "Title": "Settings", "PageType": "CCEM.Views.SCCM.AgentSettingsPage" },
            { "Id": "InstallRepair", "Title": "Install/Repair", "PageType": "CCEM.Views.SCCM.InstallRepairPage" }
          ]
        },
        {
          "Id": "SCCMInventory",
          "Title": "Inventory",
          "Glyph": "\uE8F1",
          "HasSubItems": true,
          "SubItems": [
            { "Id": "Cache", "Title": "Cache", "PageType": "CCEM.Views.SCCM.CachePage" },
            { "Id": "Applications", "Title": "Applications", "PageType": "CCEM.Views.SCCM.ApplicationsPage" },
            { "Id": "InstalledSoftware", "Title": "Installed Software", "PageType": "CCEM.Views.SCCM.InstalledSoftwarePage" }
          ]
        },
        {
          "Id": "SCCMUpdates",
          "Title": "Software Updates",
          "Glyph": "\uE895",
          "HasSubItems": true
        },
        {
          "Id": "SCCMSystem",
          "Title": "System",
          "Glyph": "\uE770",
          "HasSubItems": true
        },
        {
          "Id": "SCCMTools",
          "Title": "Tools",
          "Glyph": "\uE90F",
          "HasSubItems": true,
          "IsDynamic": true
        }
      ]
    },
    {
      "Id": "IntuneGroup",
      "Title": "Intune",
      "Glyph": "\uE753",
      "IsExpandable": true,
      "Items": [
        {
          "Id": "IntuneConnection",
          "Title": "Connection",
          "Glyph": "\uE703",
          "PageType": "CCEM.Views.Intune.ConnectionPage"
        },
        {
          "Id": "IntuneDevices",
          "Title": "Devices",
          "Glyph": "\uE977",
          "HasSubItems": true,
          "SubItems": [
            { "Id": "AllDevices", "Title": "All Devices", "PageType": "CCEM.Views.Intune.DevicesPage" },
            { "Id": "Compliance", "Title": "Compliance", "PageType": "CCEM.Views.Intune.CompliancePage" }
          ]
        },
        {
          "Id": "IntuneApps",
          "Title": "Applications",
          "Glyph": "\uE8B7",
          "HasSubItems": true
        },
        {
          "Id": "IntunePolicies",
          "Title": "Policies",
          "Glyph": "\uE8CB",
          "HasSubItems": true
        }
      ]
    }
  ],
  "FooterItems": [
    {
      "Id": "Settings",
      "Title": "Settings",
      "Glyph": "\uE713",
      "PageType": "CCEM.Views.SettingsPage"
    },
    {
      "Id": "About",
      "Title": "About",
      "Glyph": "\uE946",
      "PageType": "CCEM.Views.AboutPage"
    }
  ]
}
```

---

### Implementation Details

**Selected Structure**: Platform-First Navigation

**DevWinUI NavigationView Layout**:

```json
{
  "Groups": [
    {
      "Id": "DashboardGroup",
      "Title": "Dashboard",
      "Glyph": "\uE80F",
      "Items": [
        {
          "Id": "Connection",
          "Title": "Connection",
          "Glyph": "\uE703",
          "PageType": "CCEM.Views.SCCM.ConnectionPage",
          "Description": "Connect to SCCM client"
        },
        {
          "Id": "Overview",
          "Title": "Overview",
          "Glyph": "\uE8BC",
          "PageType": "CCEM.Views.SCCM.OverviewPage",
          "Description": "Client status overview"
        }
      ]
    },
    {
      "Id": "AgentGroup",
      "Title": "Agent Management",
      "Glyph": "\uE950",
      "Items": [
        {
          "Id": "Components",
          "Title": "Components",
          "Glyph": "\uE9F9",
          "PageType": "CCEM.Views.SCCM.ComponentsPage"
        },
        {
          "Id": "Settings",
          "Title": "Settings",
          "Glyph": "\uE713",
          "PageType": "CCEM.Views.SCCM.AgentSettingsPage"
        },
        {
          "Id": "InstallRepair",
          "Title": "Install/Repair",
          "Glyph": "\uE90F",
          "PageType": "CCEM.Views.SCCM.InstallRepairPage"
        },
        {
          "Id": "InstallAgent",
          "Title": "Install Agent",
          "Glyph": "\uE896",
          "PageType": "CCEM.Views.SCCM.InstallAgentPage"
        }
      ]
    },
    {
      "Id": "InventoryGroup",
      "Title": "Inventory",
      "Glyph": "\uE8F1",
      "Items": [
        {
          "Id": "Cache",
          "Title": "Cache",
          "Glyph": "\uE895",
          "PageType": "CCEM.Views.SCCM.CachePage"
        },
        {
          "Id": "Applications",
          "Title": "Applications",
          "Glyph": "\uE8B7",
          "PageType": "CCEM.Views.SCCM.ApplicationsPage"
        },
        {
          "Id": "InstalledSoftware",
          "Title": "Installed Software",
          "Glyph": "\uE74C",
          "PageType": "CCEM.Views.SCCM.InstalledSoftwarePage"
        },
        {
          "Id": "Advertisements",
          "Title": "Advertisements",
          "Glyph": "\uE789",
          "PageType": "CCEM.Views.SCCM.AdvertisementsPage"
        },
        {
          "Id": "ExecutionHistory",
          "Title": "Execution History",
          "Glyph": "\uE81C",
          "PageType": "CCEM.Views.SCCM.ExecutionHistoryPage"
        }
      ]
    },
    {
      "Id": "SoftwareUpdatesGroup",
      "Title": "Software Updates",
      "Glyph": "\uE895",
      "Items": [
        {
          "Id": "Updates",
          "Title": "Updates",
          "Glyph": "\uE777",
          "PageType": "CCEM.Views.SCCM.UpdatesPage"
        },
        {
          "Id": "AllUpdates",
          "Title": "All Updates",
          "Glyph": "\uE8FD",
          "PageType": "CCEM.Views.SCCM.AllUpdatesPage"
        },
        {
          "Id": "UpdateStatus",
          "Title": "Update Status",
          "Glyph": "\uE7BA",
          "PageType": "CCEM.Views.SCCM.UpdateStatusPage"
        }
      ]
    },
    {
      "Id": "SystemGroup",
      "Title": "System Management",
      "Glyph": "\uE770",
      "Items": [
        {
          "Id": "Services",
          "Title": "Services",
          "Glyph": "\uE950",
          "PageType": "CCEM.Views.SCCM.ServicesPage"
        },
        {
          "Id": "Processes",
          "Title": "Processes",
          "Glyph": "\uE9D9",
          "PageType": "CCEM.Views.SCCM.ProcessesPage"
        },
        {
          "Id": "Logs",
          "Title": "Logs",
          "Glyph": "\uE8A5",
          "PageType": "CCEM.Views.SCCM.LogsPage"
        },
        {
          "Id": "LogViewer",
          "Title": "Log Viewer",
          "Glyph": "\uE8A5",
          "PageType": "CCEM.Views.SCCM.LogViewerPage"
        },
        {
          "Id": "EventMonitoring",
          "Title": "Event Monitoring",
          "Glyph": "\uE7C1",
          "PageType": "CCEM.Views.SCCM.EventMonitoringPage"
        }
      ]
    },
    {
      "Id": "AdvancedGroup",
      "Title": "Advanced",
      "Glyph": "\uE9E9",
      "Items": [
        {
          "Id": "ServiceWindows",
          "Title": "Service Windows",
          "Glyph": "\uE787",
          "PageType": "CCEM.Views.SCCM.ServiceWindowsPage"
        },
        {
          "Id": "Variables",
          "Title": "Variables",
          "Glyph": "\uE8CB",
          "PageType": "CCEM.Views.SCCM.VariablesPage"
        },
        {
          "Id": "Power",
          "Title": "Power Management",
          "Glyph": "\uE945",
          "PageType": "CCEM.Views.SCCM.PowerPage"
        },
        {
          "Id": "WMIBrowser",
          "Title": "WMI Browser",
          "Glyph": "\uE8B7",
          "PageType": "CCEM.Views.SCCM.WMIBrowserPage"
        },
        {
          "Id": "Evaluation",
          "Title": "Evaluation",
          "Glyph": "\uE9D5",
          "PageType": "CCEM.Views.SCCM.EvaluationPage"
        },
        {
          "Id": "ImportApp",
          "Title": "Import Application",
          "Glyph": "\uE8B5",
          "PageType": "CCEM.Views.SCCM.ImportAppPage"
        }
      ]
    },
    {
      "Id": "PluginsGroup",
      "Title": "Tools & Plugins",
      "Glyph": "\uE90F",
      "Visibility": "Dynamic",
      "Items": [
        {
          "Id": "RDP",
          "Title": "Remote Desktop",
          "Glyph": "\uE8AF",
          "PluginId": "CCEM.SCCM.Plugins.RDP"
        },
        {
          "Id": "CompMgmt",
          "Title": "Computer Management",
          "Glyph": "\uE7F8",
          "PluginId": "CCEM.SCCM.Plugins.ComputerManagement"
        },
        {
          "Id": "RemoteTools",
          "Title": "Remote Tools",
          "Glyph": "\uE90A",
          "PluginId": "CCEM.SCCM.Plugins.RemoteTools"
        },
        {
          "Id": "ResourceExplorer",
          "Title": "Resource Explorer",
          "Glyph": "\uE8B7",
          "PluginId": "CCEM.SCCM.Plugins.ResourceExplorer"
        },
        {
          "Id": "StatusMessageViewer",
          "Title": "Status Message Viewer",
          "Glyph": "\uE8F2",
          "PluginId": "CCEM.SCCM.Plugins.StatusMessageViewer"
        },
        {
          "Id": "Explorer",
          "Title": "File Explorer",
          "Glyph": "\uE8B7",
          "PluginId": "CCEM.SCCM.Plugins.FileExplorer"
        },
        {
          "Id": "Regedit",
          "Title": "Registry Editor",
          "Glyph": "\uE8B7",
          "PluginId": "CCEM.SCCM.Plugins.RegistryEditor"
        },
        {
          "Id": "MSInfo32",
          "Title": "System Information",
          "Glyph": "\uE946",
          "PluginId": "CCEM.SCCM.Plugins.SystemInfo"
        },
        {
          "Id": "MSRA",
          "Title": "Remote Assistance",
          "Glyph": "\uE8AF",
          "PluginId": "CCEM.SCCM.Plugins.RemoteAssistance"
        },
        {
          "Id": "PSScripts",
          "Title": "PowerShell Scripts",
          "Glyph": "\uE756",
          "PluginId": "CCEM.SCCM.Plugins.PowerShellScripts"
        }
      ]
    },
    {
      "Id": "IntuneGroup",
      "Title": "Intune",
      "Glyph": "\uE753",
      "Visibility": "ComingSoon",
      "Items": [
        {
          "Id": "IntuneOverview",
          "Title": "Coming Soon",
          "Glyph": "\uE823",
          "PageType": "CCEM.Views.Intune.ComingSoonPage"
        }
      ]
    }
  ]
}
```

**Navigation Structure Explanation**:

1. **Dashboard Group** - Points d'entrée principaux
   - Connection: Gestion de la connexion client SCCM
   - Overview: Vue d'ensemble du statut client

2. **Agent Management** - Gestion de l'agent SCCM
   - Components: Composants de l'agent
   - Settings: Paramètres de l'agent
   - Install/Repair: Installation/réparation
   - Install Agent: Assistant d'installation

3. **Inventory Group** - Inventaire et applications
   - Cache: Gestion du cache
   - Applications: Applications déployées
   - Installed Software: Logiciels installés
   - Advertisements: Déploiements
   - Execution History: Historique d'exécution

4. **Software Updates** - Mises à jour logicielles
   - Updates: Mises à jour disponibles
   - All Updates: Toutes les mises à jour
   - Update Status: Statut des mises à jour

5. **System Management** - Gestion système
   - Services: Services Windows
   - Processes: Processus en cours
   - Logs: Fichiers de logs
   - Log Viewer: Visualiseur de logs
   - Event Monitoring: Surveillance des événements

6. **Advanced Group** - Fonctionnalités avancées
   - Service Windows: Fenêtres de maintenance
   - Variables: Variables de collection
   - Power Management: Gestion de l'alimentation
   - WMI Browser: Navigateur WMI
   - Evaluation: Règles d'évaluation
   - Import Application: Import d'applications

7. **Plugins Group** - Outils et extensions
   - Dynamiquement peuplé par les plugins chargés
   - RDP, Computer Management, Remote Tools, etc.

8. **Intune Group** - Préparation future
   - Coming Soon placeholder

```csharp
// In App.xaml.cs - Load navigation
private void LoadNavigationMenu()
{
    var json = File.ReadAllText("Assets/NavViewMenu/AppData.json");
    var navData = JsonSerializer.Deserialize<NavigationData>(json);

    // DevWinUI NavigationView binding
    MainWindow.NavigationView.MenuItemsSource = navData.Groups;
    MainWindow.NavigationView.FooterMenuItemsSource = navData.FooterItems;
}
```

### 2.2 Create Connection UI

**Status**: ✅ Completed
**New File**: `Views/SCCM/ConnectionPage.xaml`

- [x] Create XAML page with WinUI 3 controls
- [x] ✅ Use `AutoSuggestBox` (not WPF AutoCompleteBox)
- [x] ✅ Use `Button` for Connect/Disconnect
- [x] ✅ Use DevWinUI `SettingsCard` for modern UI
- [x] ✅ Use `InfoBar` for status messages
- [x] Create `ConnectionViewModel.cs`
- [x] Register ViewModel in DI

**WPF Conversion**:

- ✅ Remove Ribbon connection panel (not applicable - new page)
- ✅ Replace with modern WinUI 3 DevWinUI controls
- ✅ `AutoCompleteBox` → `AutoSuggestBox`

**Files Created**:
- `Views/SCCM/ConnectionPage.xaml` - Connection UI page
- `Views/SCCM/ConnectionPage.xaml.cs` - Code-behind
- `ViewModels/SCCM/ConnectionViewModel.cs` - ViewModel with connection logic
- Registered `ConnectionViewModel` in `App.xaml.cs`

### 2.3 Main Window Updates

**Status**: ✅ Completed
**File**: `MainWindow.xaml`

- [x] Keep existing DevWinUI `NavigationView`
- [x] Add connection status indicator in TitleBar
- [x] Subscribe to connection events in code-behind
- [x] Add plugin menu items dynamically (prepared for Phase 4)

**Implementation**:
- Added connection status indicator in `TitleBar.RightHeader`
- Real-time status updates using `ISCCMConnectionService` events
- Visual feedback with icon and text (Green = Connected, Gray = Disconnected)

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
