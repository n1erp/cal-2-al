# Cal2AL

[![CI](https://github.com/n1erp/cal-2-al/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/n1erp/cal-2-al/actions/workflows/ci.yml)
[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://www.apache.org/licenses/LICENSE-2.0)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-CLI-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-0078D4)](#release-packages)
[![Dynamics NAV](https://img.shields.io/badge/Dynamics-NAV%20C%2FAL-00A1F1)](https://learn.microsoft.com/dynamics-nav/)
[![Business Central](https://img.shields.io/badge/Business%20Central-AL-00A1F1)](https://learn.microsoft.com/dynamics365/business-central/dev-itpro/developer/devenv-programming-in-al)

Cal2AL converts Dynamics NAV C/AL objects exported as text (`.txt`, `.delta`) into AL for Business Central. It is a clean-room re-implementation of the behaviour of Microsoft's Txt2Al.

## Installation

Download the latest Cal2AL release from GitHub:

**Windows (x64)**

[Download Cal2AL for Windows](https://github.com/n1erp/cal-2-al/releases/download/0.1.0/cal2al-0.1.0-win-x64.zip)

1. Extract the ZIP archive.
2. Open PowerShell in the extracted folder.
3. Run `.\cal2al.exe --help` to get started.

**Linux (x64)**

[Download Cal2AL for Linux](https://github.com/n1erp/cal-2-al/releases/download/0.1.0/cal2al-0.1.0-linux-x64.tar.gz)

1. Download and extract the archive.
2. Make the executable runnable if necessary:

   ```bash
   chmod +x cal2al
   ```

3. Run `./cal2al --help` to get started.

**Requirements:** Cal2AL is distributed as a self-contained executable. No separate .NET installation is required.

For future versions, visit the [GitHub Releases page](https://github.com/n1erp/cal-2-al/releases).

## Quick Start

Cal2AL provides several commands to help developers inspect, validate, analyze, convert, and compare NAV objects.

### A complete command-line workflow

Cal2AL is designed for developers and migration teams who want full control over the conversion process.

**Convert objects**

Windows:
```powershell
.\cal2al.exe convert --source C:\Export --target C:\AL\MyApp
```

Linux:
```bash
./cal2al convert --source /home/user/Export --target /home/user/AL/MyApp
```

**Analyze before conversion**

Windows:
```powershell
.\cal2al.exe analyze --source C:\Export --verbose
```

Linux:
```bash
./cal2al analyze --source /home/user/Export --verbose
```

**Validate source objects**

Windows:
```powershell
.\cal2al.exe validate --source C:\Export
```

Linux:
```bash
./cal2al validate --source /home/user/Export
```

**Inspect an individual object**

Windows:
```powershell
.\cal2al.exe inspect --source C:\Export --object Codeunit:80 --al
```

Linux:
```bash
./cal2al inspect --source /home/user/Export --object Codeunit:80 --al
```

**Generate a JSON migration report**

Windows:
```powershell
.\cal2al.exe convert `
    --source C:\Export `
    --target C:\AL\MyApp `
    --report C:\AL\report.json
```

Linux:
```bash
./cal2al convert \
    --source /home/user/Export \
    --target /home/user/AL/MyApp \
    --report /home/user/AL/report.json
```

The generated output includes AL files, extracted layouts, translations, generated supporting objects, migration reports, and original source artifacts for unsupported constructs.

For all available options and arguments:

```powershell
.\cal2al.exe --help
```

Linux:
```bash
./cal2al --help
```


WinGet updates are available once the corresponding release manifest has been published and approved.

## Release Artifacts

Each GitHub release provides platform-specific downloads:

- `cal2al-0.1.0-win-x64.zip` – Windows x64 executable (`cal2al.exe`), README and LICENSE.
- `cal2al-0.1.0-linux-x64.tar.gz` – Linux x64 executable (`cal2al`), README and LICENSE.
- `SHA256SUMS.txt` – Checksums for verifying downloaded packages.

Verify downloaded files on Linux using:

```bash
sha256sum -c SHA256SUMS.txt
```

On Windows PowerShell, verify an archive using:

```powershell
Get-FileHash .\cal2al-0.1.0-win-x64.zip -Algorithm SHA256
```

Compare the resulting hash with the corresponding entry in `SHA256SUMS.txt`.

The executables are self-contained single files, so no separate .NET installation is needed.

## Documentation

- [Architecture](docs/architecture.md)
- [Conversion pipeline](docs/conversion-pipeline.md)
- [Supported features](docs/supported-features.md)
- [Known limitations](docs/known-limitations.md)
- [Command-line reference](docs/command-line-reference.md)

## License

Licensed under the [Apache License, Version 2.0](LICENSE).
