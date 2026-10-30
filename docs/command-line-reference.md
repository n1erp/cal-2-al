# Command-line reference

```
cal2al [command] [options]
```

Global options (all commands): `--verbose`/`-v` (all diagnostics, resolutions and a per-object table), `--quiet`/`-q` (errors and the result line only), `--no-color` (also `NO_COLOR` env var), `--help`/`-h`, `--version`.

## convert

Convert C/AL text exports into an AL project folder.

```
cal2al convert --source <file|folder> --target <folder> [options]
```

| Option | Default | Meaning |
|---|---|---|
| `-s, --source` | (required) | `.txt`/`.delta` file, or folder searched recursively |
| `-t, --target` | (required) | Output folder (must not be inside the source folder) |
| `-r, --report` | `<target>/.cal2al/report.json` | JSON report path |
| `--fail-on` | `Error` | `Error`, `Warning` or `Never`: when to exit with code 1 |
| `--types, --object-types` | all | e.g. `Table,Page` |
| `--ids` | all | e.g. `50000..50099,50200` |
| `--structure` | `Mirror` | `Mirror`, `ByType`, `Flat` |
| `--extension-start-id` | `70000000` | First ID for table/page extensions |
| `--runtime` | `12.0` | Target AL runtime |
| `--encoding` | `850` | Code page for files that are not UTF-8 |
| `--date-order` | `MonthDayYear` | Day/month order of C/AL date constants (`DayMonthYear` for day-first exports) |
| `--scope-attributes` | off | Add `[Scope('OnPrem')]` like Txt2Al |
| `--no-metadata` | off | Omit the Version List/Date/Time header comment |
| `--no-translations` | off | Do not write XLIFF files |
| `--no-verify` | off | Skip the token-by-token code preservation check |
| `--file-pattern` | `{name}.{type}` | Tokens `{name}` `{type}` `{id}` |
| `--extension-file-pattern` | `{name}.{type}` | Tokens `{name}` `{type}` `{id}` `{targetName}` |

Output: `.al` files, `layouts/`, enum objects, `DotNet.Package.al`, `Translations/Cal2AL.<lang>.xlf`, `.cal2al/artifacts/`, `.cal2al/report.json`.

## analyze

Convert in memory; print diagnostics and statistics; write nothing except `--report` if given.
Options: `--source`, `--report`, `--fail-on`, `--types`, `--ids`, `--runtime`, `--encoding`, `--date-order`, `--scope-attributes`, `--no-verify`.

## validate

Check that input can be read and parsed (TXT structure and C/AL code syntax) without converting.
Options: `--source`, `--report`, `--fail-on`, `--encoding`.

## inspect

```
cal2al inspect --source <file|folder>                 # table of objects with status and counts
cal2al inspect --source <file> --object Table:50000   # parsed structure tree
cal2al inspect --source <file> --object Page:21 --al  # converted AL and its diagnostics
cal2al inspect --diagnostics                          # catalogue of diagnostic codes
```

## compare

```
cal2al compare --cal2al <folder> --reference <folder> [--report <file>] [--show-equivalent] [-v]
```

Pairs objects by type and ID and classifies every property, variable, method and element as `Equivalent`, `Formatting`, `Behavioral`, `MissingInCal2AL` or `MissingInReference`. Exit code 1 when anything is `Behavioral` or `MissingInCal2AL`. Use `tools/New-Txt2AlReference.ps1` to produce the reference with Txt2Al.

## Exit codes

| Code | Meaning |
|---|---|
| 0 | Success (no diagnostic at the `--fail-on` level) |
| 1 | Diagnostics at the `--fail-on` level (or differences, for `compare`) |
| 2 | Invalid command line |
| 3 | Source not found or no objects found |
| 4 | Unexpected error |
| 130 | Cancelled (Ctrl+C) |

## Diagnostic code ranges

`CAL0xxx` input and TXT parsing · `CAL1xxx` object structure · `CAL2xxx` properties · `CAL3xxx` code · `CAL4xxx` references · `CAL5xxx` delta files · `CAL9xxx` output. Each diagnostic has object, member, element, file/line, construct, reason and suggested resolution. Full list: `cal2al inspect --diagnostics`.
