# AGENTS.md (Repo Instructions for Coding Agents)

This repository contains **CCEM (Client Center for Endpoint Manager)**, a WinUI 3 desktop app and supporting libraries.
Most code lives under `src/` and targets **.NET 10 + Windows** (`net10.0-windows10.0.26100.0`, x64).

## Repo Layout
- `src/CCEM/` — WinUI app (MVVM, WinUI 3)
- `src/CCEM.Core.*` — supporting libraries (Velopack update integration, SCCM helpers, logging)
- `src/CCEM.Core.Velopack.Tests/` — xUnit tests
- `src/Directory.Build.props` / `src/Directory.Build.targets` — shared MSBuild settings
- `src/.editorconfig` — formatting + Roslyn/IDE + ReSharper settings
- `src/settings.xamlstyler` — XAML Styler formatting rules

## Build / Lint / Test Commands

### Prereqs
- Windows machine (projects target Windows TFMs).
- .NET SDK: **10.0.x** (CI uses `10.0.x`).
- Windows SDK: `10.0.26100.*` (see `src/Directory.Build.props`).
- For WinUI + T4: Visual Studio 2022 is recommended.

### Restore
- `dotnet restore src/CCEM/CCEM.csproj`
- `dotnet restore src/CCEM.Core.Velopack.Tests/CCEM.Core.Velopack.Tests.csproj`

### Build
- App (Debug): `dotnet build src/CCEM/CCEM.csproj -c Debug -p:Platform=x64`
- App (Release): `dotnet build src/CCEM/CCEM.csproj -c Release -p:Platform=x64`
- Library: `dotnet build src/CCEM.Core.Velopack/CCEM.Core.Velopack.csproj -c Debug -p:Platform=x64`

### Run (local)
- `dotnet run --project src/CCEM/CCEM.csproj -c Debug -p:Platform=x64`
- If `dotnet run` can’t launch WinUI correctly, open `src/CCEM.slnx` in Visual Studio 2022 and press F5.

### MSBuild (optional)
If you’re using Visual Studio Build Tools / `msbuild` directly:
- `msbuild src/CCEM/CCEM.csproj /restore /p:Configuration=Debug /p:Platform=x64`
- `msbuild src/CCEM/CCEM.csproj /restore /p:Configuration=Release /p:Platform=x64`

### Lint / Code Style (C#)
There is no separate lint script; style is enforced via:
- `src/.editorconfig`
- `EnforceCodeStyleInBuild=true` in `src/Directory.Build.props`

Practical “lint” command:
- `dotnet build src/CCEM/CCEM.csproj -c Debug -p:Platform=x64`

Optional (only if installed in your environment):
- `dotnet format` (apply `.editorconfig` formatting/analyzers)

### Test
- Run all tests: `dotnet test src/CCEM.Core.Velopack.Tests/CCEM.Core.Velopack.Tests.csproj -c Debug -p:Platform=x64`
- List tests (discover names for filtering): `dotnet test src/CCEM.Core.Velopack.Tests/CCEM.Core.Velopack.Tests.csproj -c Debug --list-tests`

### Run a Single Test (xUnit)
Use `dotnet test --filter` with `FullyQualifiedName`.

Examples:
- Single method:
  - `dotnet test src/CCEM.Core.Velopack.Tests/CCEM.Core.Velopack.Tests.csproj -c Debug --filter "FullyQualifiedName=CCEM.Core.Velopack.Tests.Unit.VelopackUpdateServiceTests.Constructor_Throws_WhenConfigurationIsNull"`
- All tests in a class:
  - `dotnet test src/CCEM.Core.Velopack.Tests/CCEM.Core.Velopack.Tests.csproj -c Debug --filter "FullyQualifiedName~VelopackUpdateServiceIntegrationTests"`
- All unit tests namespace:
  - `dotnet test src/CCEM.Core.Velopack.Tests/CCEM.Core.Velopack.Tests.csproj -c Debug --filter "FullyQualifiedName~CCEM.Core.Velopack.Tests.Unit"`

### Coverage (optional)
The test project references `coverlet.collector`.
- `dotnet test src/CCEM.Core.Velopack.Tests/CCEM.Core.Velopack.Tests.csproj -c Debug --collect:"XPlat Code Coverage"`

### Publish (unpackaged) (matches CI intent)
CI publishes for Velopack with win-x64 + self-contained:
- `dotnet publish src/CCEM/CCEM.csproj -c Release -r win-x64 --self-contained true -o artifacts/publish`

### Versioning (CI)
- CI updates version fields in `src/CCEM/Package.appxmanifest` and `src/SharedAssemblyInfo.cs`.
- Don’t introduce new version constants elsewhere unless there’s a strong reason.

## Code Style Guidelines

### Formatting (from `src/.editorconfig`)
- Line endings: **CRLF**
- Encoding: **UTF-8 with BOM**
- Indentation:
  - `*.cs`, `*.xaml`, etc: **4 spaces**
  - XML-ish files (`*.csproj`, `*.props`, `*.targets`, `*.json`, `*.appxmanifest`, …): **2 spaces**
- Always keep final newline; trim trailing whitespace.

### Namespaces
- Prefer **file-scoped namespaces** (configured as warning):
  - `namespace My.Namespace;`

### Imports / Usings
- Keep `using` directives sorted with **System first** (`dotnet_sort_system_directives_first=true`).
- Keep imports minimal; remove unused usings.
- Implicit usings are enabled (`ImplicitUsings=enable`), so don’t add redundant framework usings.

### Types, Nullability, and APIs
- Nullable reference types are enabled (`<Nullable>enable</Nullable>`). Fix warnings rather than suppressing.
- Avoid `null!` except in tests or when bridging external APIs.
- Prefer clear guard clauses:
  - `if (arg is null) throw new ArgumentNullException(nameof(arg));`
  - or `ArgumentNullException.ThrowIfNull(arg);` when appropriate.
- Prefer explicit accessibility (`public`, `private`, …) for non-interface members.

### Naming Conventions
- Follow standard .NET naming:
  - Types/methods/properties/events: `PascalCase`
  - Parameters/locals: `camelCase`
  - Private fields: `_camelCase` (common in this repo)
  - Async methods: `*Async`
- Tests follow the pattern `Method_Condition_ExpectedResult` (keep this style).

### Control Flow & Expressions
- Prefer braces even for single-line statements (`csharp_prefer_braces=true`).
- Use `var` when the type is obvious; otherwise use explicit types (repo settings allow `var` broadly).
- Prefer modern C# features when they improve clarity (project uses `LangVersion=latest`).

## Error Handling & Logging
- Library code: avoid catching `Exception` unless you can add actionable context or are doing best-effort cleanup.
- UI/ViewModel layer: catching and surfacing an error message is acceptable; prefer not to swallow silently.
- Don’t log secrets (tokens, access keys). In update code, treat access tokens as sensitive.
- Prefer structured logging if using `CCEM.Core.Logger` (Serilog).

## WinUI / MVVM Conventions
- The app uses `CommunityToolkit.Mvvm`:
  - Use `[RelayCommand]` for commands.
  - Prefer observable properties/source generation when already used in that area.
  - Keep UI thread/dispatcher considerations in mind when updating bound properties.

## XAML Conventions
- If you touch XAML, keep formatting consistent with `src/settings.xamlstyler`.
- XAML Styler is configured to:
  - reorder attributes,
  - keep one attribute per line,
  - use 4-space indentation,
  - format markup extensions (except `x:Bind`/`Binding`).

## Generated Code / T4 Templates
- `src/CCEM/CCEM.csproj` contains `.tt` templates that generate `.cs` files.
- Avoid hand-editing generated `.cs` outputs; edit the `.tt` instead.
- The build target `TransformAllT4Templates` only runs if `TextTransform.exe` is available (usually via Visual Studio).

## Build Troubleshooting Notes
- If build fails due to missing `Microsoft.Management.Deployment` namespace, see comment in `src/Directory.Build.props`:
  - Adjust `WindowsSdkPackageVersion` **slightly up/down** and rebuild.

## Cursor / Copilot Rules
- No `.cursorrules`, `.cursor/rules/`, or `.github/copilot-instructions.md` files were found in this repo at the time this AGENTS.md was generated.
