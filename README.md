# Cal2AL

Cal2AL converts Dynamics NAV C/AL objects exported as text (`.txt`, `.delta`) into AL for Business Central. It is a clean-room re-implementation of the behaviour of Microsoft's Txt2Al with these goals:

* **No silent loss.** Code is carried over token for token and verified; anything that cannot be converted produces a diagnostic with object, member, location, construct, reason and suggested resolution, and the original text is saved as a migration artifact.
* **Correct AL where Txt2Al is wrong or incomplete:** date constants rewritten to AL format, classic and new-syntax exports, safe delta code changes turned into extension triggers, deterministic naming, one broken object never loses a whole file.
* **Evidence.** Per-object statement and database-operation counts (input vs output), JSON report, approved-output tests, and a `compare` command to measure differences against Txt2Al output.

## Quick start

Requires the .NET 10 SDK.

```powershell
dotnet build Cal2AL.slnx
dotnet run --project cal2al -- convert --source C:\Exports --target C:\AL\MyApp
dotnet test Cal2AL.slnx
```

Publish a single executable: `dotnet publish cal2al -c Release -r win-x64 --self-contained -p:PublishSingleFile=true`.

```text
cal2al convert  -s <src> -t <out>     convert and write an AL project
cal2al analyze  -s <src>              dry run: diagnostics + statistics
cal2al validate -s <src>              parse-only check of the input
cal2al inspect  -s <src> [-o Table:18 [--al]] | --diagnostics
cal2al compare  --cal2al <out> --reference <txt2al-out>
```

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