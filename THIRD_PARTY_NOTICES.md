# Third-Party Notices

This file contains notices for third-party code and libraries used in this project.

---

## SCCMCliCtr - Client Center for Configuration Manager

**Component:** SCCM Automation Library

**Location in repository:** `src/CCEM/SCCM/Automation/`

**Original project:** https://github.com/rzander/sccmclictr

**Author:** Roger Zander

**Copyright:** © 2018 Roger Zander

**License:** Microsoft Public License (Ms-PL)

### Description

The SCCM automation functionality in this project is derived from the SCCMCliCtr project, which provides comprehensive SCCM client management capabilities through PowerShell remoting. The code has been migrated from .NET Framework 4.8 to .NET 9 and adapted for modern .NET architecture.

### Files Derived from SCCMCliCtr

The following source files in `src/CCEM/SCCM/Automation/` are derived from SCCMCliCtr:

- `SCCMAgent.cs` - Main agent connection and runspace management
- `AgentActions.cs` - Inventory, policy, and update actions
- `AgentProperties.cs` - Agent properties and settings
- `SoftwareDistribution.cs` - Software deployment management
- `Components.cs` - SCCM component management
- `Policy.cs` - Policy management and evaluation
- `Inventory.cs` - Hardware and software inventory
- `SoftwareUpdates.cs` - Software update management
- `Health.cs` - Client health monitoring
- `Services.cs` - Windows service management
- `Monitoring.cs` - Event and log monitoring
- `LocationServices.cs` - Location services management
- `Processes.cs` - Process management
- `SWCache.cs` - Software cache management
- `DCM.cs` - Desired Configuration Management
- `AppV.cs` - App-V client management
- `WSMan.cs` - WS-Management configuration
- `ScheduleDecoding.cs` - Schedule string decoding
- `BaseInit.cs` - Base initialization classes
- `Common.cs` - Common utility functions
- `DDRGen.cs` - Discovery Data Record generation
- `Properties/Resources.Designer.cs` - Resource management
- `Properties/Settings.Designer.cs` - Settings management

### Modifications Made

The following modifications were applied during migration:

1. **Namespace changes:** `sccmclictr.automation` → `CCEM.SCCM.Automation`
2. **Target framework:** .NET Framework 4.8 → .NET 9 (`net9.0-windows10.0.26100.0`)
3. **PowerShell SDK:** Updated to PowerShell 7+ (`Microsoft.PowerShell.SDK 7.5.3`)
4. **Type disambiguation:** Qualified `System.Drawing.Image` to avoid conflicts with WinUI controls
5. **Settings integration:** Adapted to work with `nucs.JsonSettings` configuration
6. **Architecture adaptation:** Integrated with MVVM services and dependency injection

### License Note - Important Inconsistency

⚠️ **The original SCCMCliCtr project contains a licensing inconsistency:**

- **GitHub repository LICENSE.md:** Declares **Microsoft Public License (Ms-PL)**
- **Source code headers:** Declare **GNU Lesser General Public License v3+ (LGPL)**

Example from original source code headers:
```
//SCCM Client Center Automation Library (SCCMCliCtr.automation)
//Copyright (c) 2018 by Roger Zander
//
//This program is free software; you can redistribute it and/or modify it
//under the terms of the GNU Lesser General Public License as published by
//the Free Software Foundation; either version 3 of the License, or any later version.
```

**Our decision:** We have chosen to respect the **Ms-PL license** as declared in the official GitHub repository LICENSE file, as this represents the clear intent of the project owner. If you have concerns about this inconsistency, we recommend contacting Roger Zander directly.

### Full Ms-PL License Text

See [LICENSE_Ms-PL.txt](LICENSE_Ms-PL.txt) for the complete Microsoft Public License text.

### Attribution Requirements

Per Ms-PL Section 3(C), we retain all copyright, patent, trademark, and attribution notices present in the original software.

---

## Questions or Concerns?

If you have questions about third-party licenses or attributions:

1. Open an issue on the GitHub repository
2. Review the [LICENSE](LICENSE) file for the dual licensing structure
3. Contact the project maintainer

---

**Last updated:** 2025-10-04

**Thank you to all open-source contributors!** This project builds on the excellent work of many developers in the .NET and Windows development community.
