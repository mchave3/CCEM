# Client Center for Endpoint Manager

A support and troubleshooting tool for Microsoft Endpoint Manager, covering both Configuration Manager (MECM) and Intune.

**Modern WinUI 3 reimagining of the classic SCCMCliCtr tool.**

## Overview

CCEM modernizes the excellent [Client Center for Configuration Manager](https://github.com/rzander/sccmclictr) by Roger Zander, bringing it to modern Windows with WinUI 3, .NET 9, and preparing for future Intune support.

### Key Features

- 🖥️ **Modern UI**: Built with WinUI 3 and Windows App SDK 1.8
- 🔄 **SCCM Management**: Full Configuration Manager client management capabilities
- 🔮 **Future-Ready**: Architecture designed for upcoming Intune integration
- ⚡ **Performance**: .NET 9 with optimized PowerShell remoting
- 🎨 **DevWinUI**: Beautiful, native Windows 11 design

## Project Status

🚧 **Active Development** - Phase 1 Complete (Foundation & Core Library Migration)

See [MIGRATION_PLAN.md](MIGRATION_PLAN.md) for detailed progress tracking.

## License & Attribution

This project uses a **dual license structure**:

- **All code in this repository**: **MIT License** ([LICENSE_MIT.txt](LICENSE_MIT.txt)), except third-party components listed below
- **Third-party code and derived works**: See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) for individual licenses
  - **SCCMCliCtr-derived code**: **Microsoft Public License (Ms-PL)** ([LICENSE_Ms-PL.txt](LICENSE_Ms-PL.txt))

### Attribution

This project is based on **[Client Center for Configuration Manager](https://github.com/rzander/sccmclictr)** by **Roger Zander**.

- Original project: https://github.com/rzander/sccmclictr
- Original license: Microsoft Public License (Ms-PL)
- Copyright © 2018 Roger Zander

The SCCM automation library (`src/CCEM/SCCM/Automation/`) is derived from Roger Zander's SCCMCliCtr and remains under the Ms-PL license. All other content in this repository is © 2025 Mickaël CHAVE, licensed under MIT.

**Thank you to Roger Zander** for creating and maintaining SCCMCliCtr, which made this modernization possible! 🙏

For complete licensing details, see [LICENSE](LICENSE) and [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

## Building from Source

See [CLAUDE.md](CLAUDE.md) for detailed build instructions and architecture overview.

## Technology Stack

- **Framework**: WinUI 3 (Windows App SDK 1.8)
- **Language**: C# / .NET 9.0
- **Platform**: Windows 10+ (minimum 10.0.17763.0)
- **UI Library**: DevWinUI 9.1.0
- **MVVM**: CommunityToolkit.Mvvm 8.4.0

## Quick Start

```bash
# Clone the repository
git clone https://github.com/yourusername/CCEM.git

# Restore dependencies
dotnet restore CCEM.slnx

# Build the solution
dotnet build CCEM.slnx

# Run the application
dotnet run --project src/CCEM/CCEM.csproj
```

## Contributing

Contributions are welcome! When contributing:

1. **New code and contributions**: Contribute under the MIT license
2. **Modifications to existing SCCM.Automation code**: Must remain under Ms-PL
3. **All contributions**: Must respect both license terms

Please read the [LICENSE](LICENSE) file for details.

## Acknowledgments

**Huge thanks to Roger Zander** ([@rzander](https://github.com/rzander)) for creating and maintaining the original SCCMCliCtr project. This modernization builds upon his excellent work in SCCM client management automation.

## Support

For issues, questions, or suggestions:
- Open an issue on GitHub
- Check the [documentation](CLAUDE.md)
- Review the [migration plan](MIGRATION_PLAN.md)
