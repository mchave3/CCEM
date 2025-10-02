# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CCEM (Client Center for Endpoint Manager) is a support and troubleshooting tool for Microsoft Endpoint Manager, covering both Configuration Manager (MECM) and Intune.

## Technology Stack

- **Framework**: WinUI 3 (Windows App SDK 1.8)
- **Language**: C# / .NET 9.0
- **Platform**: Windows (minimum version 10.0.17763.0)
- **UI Library**: DevWinUI 9.1.0
- **MVVM**: CommunityToolkit.Mvvm 8.4.0
- **Logging**: Serilog
- **Settings**: nucs.JsonSettings with auto-save

## Build Commands

```bash
# Restore dependencies
dotnet restore CCEM.slnx

# Build the solution
dotnet build CCEM.slnx

# Build for specific platform
dotnet build CCEM.slnx -c Release /p:Platform=x64

# Publish for deployment (self-contained)
dotnet publish src/CCEM/CCEM.csproj -c Release -r win-x64 /p:Platform=x64 --self-contained true

# Publish using predefined profiles
dotnet publish src/CCEM/CCEM.csproj -c Release /p:PublishProfile=win-x64
```

## Architecture Overview

### Application Structure

- **Dependency Injection**: The app uses `Microsoft.Extensions.DependencyInjection` configured in [App.xaml.cs](src/CCEM/App.xaml.cs) `ConfigureServices()` method
- **Navigation**: JSON-based navigation system using DevWinUI's `IJsonNavigationService`
  - Navigation configuration is defined in [AppData.json](src/CCEM/Assets/NavViewMenu/AppData.json)
  - T4 templates auto-generate navigation mappings from AppData.json
- **Theming**: Managed by `IThemeService` from DevWinUI
- **Settings**: Application settings persist automatically using `nucs.JsonSettings` with `[GenerateAutoSaveOnChange]` attribute

### T4 Text Templates

The project uses T4 templates to auto-generate navigation code:
- [NavigationPageMappings.tt](src/CCEM/T4Templates/NavigationPageMappings.tt) - Maps page identifiers to page types
- [BreadcrumbPageMappings.tt](src/CCEM/T4Templates/BreadcrumbPageMappings.tt) - Generates breadcrumb navigation mappings

**Important**: T4 templates are automatically transformed before build. If working in Visual Studio, templates regenerate via the `TransformAllT4Templates` MSBuild target.

### Application Configuration

- **Settings Location**: `%AppData%\CCEM\{Version}\AppConfig.json`
- **Log Files**: `%AppData%\CCEM\{Version}\Log\Log.txt` (when developer mode enabled)
- **Settings Model**: [AppConfig.cs](src/CCEM/Common/AppConfig.cs) - versioned with auto-save functionality
- **Access Settings**: Via static `AppHelper.Settings` property

### Project Organization

- **Views/**: XAML pages and their code-behind
- **ViewModels/**: MVVM view models (registered in DI container)
- **Common/**: Helper classes, constants, configuration
- **Models/**: Data models (currently empty folder)
- **Themes/**: XAML resource dictionaries for styling
- **Assets/**: Images, icons, and configuration files

### Global Usings

The project uses global usings defined in [GlobalUsings.cs](src/CCEM/GlobalUsings.cs), which includes:
- DevWinUI components
- CommunityToolkit.Mvvm
- Microsoft.UI.Xaml
- Common helpers and ViewModels

## Key Development Patterns

### Adding New Pages

1. Create XAML page and code-behind in appropriate Views subfolder
2. Create corresponding ViewModel in ViewModels folder
3. Register ViewModel in `App.ConfigureServices()` if needed
4. Add page entry to [AppData.json](src/CCEM/Assets/NavViewMenu/AppData.json)
5. T4 templates will auto-generate navigation mappings on next build

### Accessing Services

Use `App.GetService<T>()` to retrieve registered services from anywhere in the application.

### Publishing

The project supports three platforms: x86, x64, and ARM64. Publish profiles are pre-configured in [src/CCEM/Properties/PublishProfiles/](src/CCEM/Properties/PublishProfiles/).

## CI/CD

GitHub Actions workflow for releases is configured but requires setup:
- Update `PROJECT_PATH` in [.github/workflows/dotnet-release.yml](.github/workflows/dotnet-release.yml) to `src/CCEM/CCEM.csproj`
- Workflow creates platform-specific zip files and GitHub releases
- Triggered manually via `workflow_dispatch`
