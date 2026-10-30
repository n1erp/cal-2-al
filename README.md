# Cal2AL

[![CI](https://github.com/n1erp/cal-2-al/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/n1erp/cal-2-al/actions/workflows/ci.yml)
[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://www.apache.org/licenses/LICENSE-2.0)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-CLI-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-0078D4)](#release-packages)
[![Dynamics NAV](https://img.shields.io/badge/Dynamics-NAV%20C%2FAL-00A1F1)](https://learn.microsoft.com/dynamics-nav/)
[![Business Central](https://img.shields.io/badge/Business%20Central-AL-00A1F1)](https://learn.microsoft.com/dynamics365/business-central/dev-itpro/developer/devenv-programming-in-al)

Cal2AL converts Dynamics NAV C/AL objects exported as text (`.txt`, `.delta`) into AL for Business Central. It is a clean-room re-implementation of the behaviour of Microsoft's Txt2Al with these goals:

* **No silent loss.** Code is carried over token for token and verified; anything that cannot be converted produces a diagnostic with object, member, location, construct, reason and suggested resolution, and the original text is saved as a migration artifact.
* **Correct AL where Txt2Al is wrong or incomplete:** date constants rewritten to AL format, classic and new-syntax exports, safe delta code changes turned into extension triggers, deterministic naming, one broken object never loses a whole file.
* **Evidence.** Per-object statement and database-operation counts (input vs output), JSON report, approved-output tests, and a `compare` command to measure differences against Txt2Al output.

## Quick start

Requires the .NET 10 SDK.

```powershell
dotnet build Cal2AL.slnx
dotnet run --project cal2al/cal2al.csproj -- convert --source C:\Exports --target C:\AL\MyApp
dotnet test Cal2AL.slnx
```

Publish a single executable: `dotnet publish cal2al/cal2al.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true`.

```text
cal2al convert  -s <src> -t <out>     convert and write an AL project
cal2al analyze  -s <src>              dry run: diagnostics + statistics
cal2al validate -s <src>              parse-only check of the input
cal2al inspect  -s <src> [-o Table:18 [--al]] | --diagnostics
cal2al compare  --cal2al <out> --reference <txt2al-out>
```

## Continuous integration

`.github/workflows/ci.yml` runs on every push and pull request to `main`:

* **test** (Ubuntu and Windows): restore, Release build, all xUnit tests (results uploaded as `.trx`), and a smoke run of `cal2al convert` on `tests/Fixtures`.
* **package** (pushes to `main` only, after the tests pass): self-contained single-file `cal2al` executables for `win-x64`, `linux-x64` and `osx-arm64`, downloadable from the run's artifacts for 30 days.

## Release packages

```powershell
./scripts/Publish-Artifacts.ps1 -Version 0.1.0          # runs the tests, then packages
./scripts/Publish-Artifacts.ps1 -Version 0.1.0 -SkipTests
```

Produces, in `artifacts/` (ignored by git):

| File | Content |
|---|---|
| `cal2al-<version>-win-x64.zip` | `cal2al.exe`, `README.md`, `LICENSE` |
| `cal2al-<version>-linux-x64.tar.gz` | `cal2al`, `README.md`, `LICENSE` |
| `SHA256SUMS.txt` | checksums, verifiable with `sha256sum -c SHA256SUMS.txt` |

The executables are self-contained single files (no .NET install needed). Upload the three files to a GitHub release, e.g. `gh release create v0.1.0 artifacts/* --title "Cal2AL 0.1.0"`. A Linux archive built on Windows has no executable bit: run `chmod +x cal2al` after extracting.

## Layout

| Path | Content |
|---|---|
| `cal2al/` | CLI (Cal2AL.CLI, executable `cal2al`) — System.CommandLine + Spectre.Console |
| `src/Cal2AL.Core` | C/AL object model |
| `src/Cal2AL.Parser` | TXT parser, encoding detection, C/AL lexer and parser, date literals |
| `src/Cal2AL.Converter` | Object converters, engine, output layout, comparison, DI registration |
| `src/Cal2AL.Generator` | AL model and writer |
| `src/Cal2AL.Diagnostics` | Diagnostic catalogue and report |
| `tests/Cal2AL.Tests` | xUnit tests; `tests/Fixtures` input, `tests/Cal2AL.Tests/Expected` approved output |
| `tools/` | `New-Txt2AlReference.ps1` (Windows) and `txt2al-on-linux/` (run Txt2Al under Mono) for comparisons |

## Documentation

* [Architecture](docs/architecture.md) — how Txt2Al works and how Cal2AL is built
* [Conversion pipeline](docs/conversion-pipeline.md)
* [Supported features](docs/supported-features.md)
* [Known limitations](docs/known-limitations.md)
* [Command-line reference](docs/command-line-reference.md)

## License

Licensed under the [Apache License, Version 2.0](LICENSE).
