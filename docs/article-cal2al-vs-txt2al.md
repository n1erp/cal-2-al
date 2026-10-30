# Cal2AL vs Txt2Al: migrating a real NAV 2016 database to AL

*How we rebuilt Microsoft's C/AL-to-AL converter, tested it on two complete NAV 2016 databases, and what we found.*

---

## Why another converter?

Every NAV-to-Business Central migration starts with the same step: export the C/AL objects as text and convert them to AL. Microsoft ships **Txt2Al** for this. It does the job, but anyone who has used it on a real database knows the routine: run it, open hundreds of files, and fix compile errors by hand. Some code is silently dropped, and one unreadable object can stop the whole conversion.

**Cal2AL** is a clean-room reimplementation of what Txt2Al does, built around three rules:

1. **Never lose business logic.** Every statement is carried over and checked.
2. **Never fail silently.** Anything that cannot be converted gets a diagnostic: object, procedure, line, what it is, why, and how to fix it. The original text is saved as a migration artifact.
3. **Prove it.** Each object's statements and database operations are counted in the input and in the output, and the counts must match.

Cal2AL is a .NET 10 command-line tool (`cal2al`) with a parser, a converter, an AL generator, a diagnostics catalogue and 145 automated tests.

> The decompiled Txt2Al 14.0 source was used only to learn its behaviour. It carries no licence, so no code was copied.

---

## Getting started

```powershell
# Build and test
dotnet build Cal2AL.slnx
dotnet test Cal2AL.slnx

# Publish a single executable for Windows
dotnet publish cal2al -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

### Export the objects from NAV

```powershell
# Classic export (works with Cal2AL)
finsql.exe Command=ExportObjects, File=C:\Export\All_Obj.txt, ServerName=.\NAVDEMO, Database="NAV2016", ntauthentication=yes

# New-syntax export (what Txt2Al expects)
finsql.exe Command=ExportToNewSyntax, File=C:\Export\All_Obj.txt, ServerName=.\NAVDEMO, Database="NAV2016", ntauthentication=yes
```

### Convert

```powershell
cal2al convert --source C:\Export --target C:\AL\MyApp
```

Useful options:

```powershell
cal2al convert -s C:\Export -t C:\AL\MyApp `
    --types Table,Page,Codeunit `      # only some object types
    --ids 50000..99999 `               # only your customisations
    --structure ByType `               # Tables\, Pages\, ... instead of mirroring the source folders
    --date-order Auto `                # detect day/month order per file (default)
    --runtime 12.0 `                   # target AL runtime
    --fail-on Warning `                # exit code 1 on warnings (for CI)
    --report C:\AL\report.json         # JSON report
```

The output folder contains:

```text
MyApp\
├── *.al                      one file per object (tables, pages, reports, codeunits, XMLports, queries, extensions)
├── layouts\*.rdlc / *.docx   report layouts extracted from the export
├── *.Enum.al                 enums generated from extensible option fields
├── DotNet.Package.al         .NET types used by DotNet variables
├── Translations\*.xlf        non-English captions and text constants
└── .cal2al\
    ├── report.json           every object, status, diagnostic and metric
    └── artifacts\            original text of anything that could not be converted
```

### Analyse before you convert

```powershell
cal2al analyze  -s C:\Export -v           # dry run: diagnostics + per-object statistics, nothing written
cal2al validate -s C:\Export              # only check that every object and every code block can be parsed
```

### Look inside an object

```powershell
cal2al inspect -s C:\Export                          # table of all objects with status and counts
cal2al inspect -s C:\Export -o Table:18              # parsed structure tree of one object
cal2al inspect -s C:\Export -o Codeunit:80 --al      # the converted AL and its diagnostics
cal2al inspect --diagnostics                         # the full diagnostic catalogue (CAL0xxx–CAL9xxx)
```

### Compare with Txt2Al

```powershell
# On Windows
txt2al.exe --source=C:\Export --target=C:\Compare\txt2al --rename
cal2al convert -s C:\Export -t C:\Compare\cal2al -q
cal2al compare --cal2al C:\Compare\cal2al --reference C:\Compare\txt2al --report C:\Compare\comparison.json -v

# Or in one step
.\tools\New-Txt2AlReference.ps1 -Txt2Al C:\txt2al\Txt2Al.exe -Source C:\Export -Work C:\Compare
```

```bash
# On Linux, Txt2Al runs under Mono with two small shims for Windows API calls
./tools/txt2al-on-linux/run-txt2al.sh ~/txt2al ~/export/All_Obj.txt ~/compare/txt2al
```

`compare` pairs every property, variable, procedure, trigger and element of each object, and sorts each pair into one of five categories:

- **Equivalent**
- **Formatting** (case, quoting, spacing only)
- **Behavioral** (different tokens)
- **MissingInCal2AL**
- **MissingInReference**

### Exit codes (for pipelines)

| Code | Meaning |
|---|---|
| 0 | Success |
| 1 | Diagnostics at the `--fail-on` level (or differences, for `compare`) |
| 2 | Invalid command line |
| 3 | Source not found / no objects |
| 4 | Unexpected error |
| 130 | Cancelled |

---

## The test: two real NAV 2016 databases

We ran both tools on two complete exports of French NAV 2016 databases:

| File | Size | Objects |
|---|---|---|
| `All_Obj_NAV2016.txt` (customised) | 154 MB | 5,028: 1,211 tables, 2,147 pages, 778 reports, 800 codeunits, 49 XMLports, 39 queries, 3 MenuSuites |
| `All_Obj_STANDARD.txt` (standard) | 136 MB | 4,676 |

Both are **classic** exports in code page 850, the format most NAV databases are exported in.

> **Fairness note.** Txt2Al's documentation asks for `ExportToNewSyntax` exports, and these were classic. Txt2Al was also run under Mono on Linux, not on Windows. Its classic-syntax problems below are therefore partly "wrong input". But that input is what most real projects start from, and Cal2AL handles both formats.

### Headline results

| | Txt2Al | Cal2AL |
|---|---|---|
| Whole 154 MB export as one file | **0 objects converted.** One system table (2000000140, `WriteProtected=Yes`) made it reject the entire file | **5,028 objects** in about 18 s |
| Export split into one file per object | 5,025 files, plus about 4,600 warnings that the export is not new syntax | — |
| Code statements, input → output | not measured | **327,184 → 327,184** (NAV2016)<br>**311,236 → 311,236** (STANDARD) |
| Database operations, input → output | not measured | **40,172 → 40,172**<br>**37,771 → 37,771** |
| Code that changed during conversion | unknown | **0** (token-by-token check) |
| Objects needing manual work (NAV2016) | unknown; you find out at compile time | 18 flagged, plus 3 MenuSuites saved as artifacts |

### Entry-by-entry comparison (NAV2016)

| Category | Entries |
|---|---|
| Equivalent | 288,359 |
| Formatting only | 11,465 |
| Behavioral difference | 64,447 |
| Only in Txt2Al output | 3,362 |
| Only in Cal2AL output | 4,185 |

Almost all behavioural differences are places where Txt2Al's output is not valid AL.

---

## What we found, with real examples

All snippets below are taken from the two tools' output for the same NAV2016 objects.

### 1. Object references written as quoted numbers (about 23,900 places)

Codeunit 80 *Sales-Post*:

```al
// Txt2Al
SalesHeader: Record "36";
procedure GetSalesLines(var NewSalesHeader: Record "36"; var NewSalesLine: Record "37"; ...)

// Cal2AL
SalesHeader: Record "Sales Header";
procedure GetSalesLines(var NewSalesHeader: Record "Sales Header"; var NewSalesLine: Record "Sales Line"; ...)
```

`Record "36"` means "a table *named* 36", so it does not compile. Cal2AL builds a symbol table from every exported object and writes names. When a referenced object is not in the export, it keeps the bare ID (`Record 36`, which is valid) and tells you once per object (CAL4001).

### 2. Old C/AL filter syntax copied into AL (about 12,700 places)

Table 18 *Customer*, field *Comment*:

```al
// Txt2Al
CalcFormula = Exist("Comment Line" WHERE (Table Name=CONST(Customer),
                                          No.=FIELD(No.)));

// Cal2AL
CalcFormula = exist("Comment Line" where("Table Name" = const(Customer), "No." = field("No.")));
```

Cal2AL parses TableRelation, CalcFormula, table views, links and query filters in both classic and new syntax, and rewrites them as AL. It also turns `CONST(Yes)` into `const(true)` when the field is a Boolean.

### 3. Controls and data items without valid names (about 10,800 places)

Page 21 *Customer Card*:

```al
// Txt2Al
systempart(;Links)
systempart(;Notes)

// Cal2AL
systempart(Control1900383207; Links)
systempart(Control1905767507; Notes)
```

Report 1 *Chart of Accounts*:

```al
// Txt2Al
dataitem(DataItem6710;Table15)

// Cal2AL
dataitem("G/L Account"; "G/L Account")
```

The report's own code refers to the data item as `"G/L Account"`, which is its implicit name in C/AL. Cal2AL keeps that name and resolves `Table15`. Every control, action and data item gets a unique AL name: from its Name property, then its source field, then its caption (with the `&` access key removed), then `Control<id>`.

### 4. Dates follow the exporting PC's regional settings

The C/AL documentation describes date constants as `MMDDYY`. Real exports don't always follow it: C/SIDE writes them in the regional order of the machine that exported them. The French exports contain 142 constants such as `31129999D` that are only valid day-first, and none that are only valid month-first.

```al
// Txt2Al: copied unchanged, not a valid AL date
EXIT(31129999D);

// Cal2AL: detected as day-first, rewritten as yyyymmddD
EXIT(99991231D);
```

Cal2AL detects the order **per file**:

1. First from constants that are only valid one way.
2. Then from the object dates (`Date=15/09/15`).
3. Otherwise it assumes month-first.

It reports what it decided (CAL0005) and rewrites every constant (CAL3017). Two-digit years are reported so you can check them.

### 5. One bad object loses everything

```text
----ERROR----
Error extracting application object from file: All_Obj_NAV2016.txt
Message: WriteProtected is not valid property for Table.
Total Time: 0:00:01.84
```

That is Txt2Al's entire output for a 5,028-object export. Cal2AL splits the export into objects first and parses each one on its own, so a broken object becomes one failed entry (CAL0101, with line and column) plus an artifact, and every other object still converts.

### 6. What Txt2Al drops

- **Disabled keys** disappear. Cal2AL keeps them with `Enabled = false`.
- **Unknown triggers** disappear. Cal2AL keeps their code as a local procedure and explains why (CAL1002).
- **Unsupported properties and objects** disappear or end up commented out. Cal2AL reports each one, and saves unsupported objects (MenuSuites, Forms, Dataports) verbatim in `.cal2al\artifacts\`.

### 7. Other Txt2Al defects seen

- The `Namespaces` property of the PEPPOL XMLports is split across lines into an invalid string.
- Captions keep their `&` in control names (`group("&Company")`).
- The output files use `./X.rdlc` layout paths mixed in with the code.

---

## What real data taught Cal2AL

The test also found bugs in Cal2AL. Each was fixed and has a regression test:

| Found in the NAV 2016 export | Fix |
|---|---|
| `TextConst` whose strings start on the next line (`TextConst` ↵ `'@@@={Locked}',` ↵ `'ENU=...'`) | parsed |
| Unnamed return values: `PROCEDURE CreateText@1(...) @1000 : Text[80];` | parsed |
| Comparisons inside sets: `IF TRUE IN [A <> B, NOT (C OR D)] THEN` | parsed |
| Day-first date constants | `--date-order Auto` |
| Word layouts encoded in padded base64 chunks, one per line | decoded line by line |
| Query `Count` columns without a source field | `column(Name) { Method = Count; }` |
| `OptionOrdinalValues=[0;1;2;3]` | `OptionOrdinalValues = 0, 1, 2, 3;` |
| Placeholder key `{ No ; }` with no fields | not emitted, reported |
| `ProviderID=1120022` | resolved to the providing part's name |
| `InitValue=[ ]` (blank option member) | `InitValue = " ";` |
| mscorlib 2.0 and 4.0 versions of the same .NET type | unified into one declaration |
| Repeated XMLport node names (`LineNo`) | `LineNo_2` with `XmlName = 'LineNo'` |

After the fixes, nothing failed to parse in either database.

### What still needs a developer (NAV2016)

| Diagnostic | Count | What to do |
|---|---|---|
| CAL3005 Automation | 102 | Replace Excel/COM automation (e.g. codeunit 7152) with Excel Buffer or .NET |
| CAL3017 invalid date | 25 | `01010000D` (year 0) is not a calendar date; decide how to express it |
| CAL1001 MenuSuite | 3 | Use UsageCategory/ApplicationArea and role centers |
| Warnings | 2,020 | mostly .NET variables (on-premises only), obsolete functions, `File` variables |

That is the real migration backlog: a list with object, line, reason and suggested fix, instead of thousands of compile errors to triage.

---

## Why Cal2AL is better than Txt2Al

| | Txt2Al | Cal2AL |
|---|---|---|
| **Input formats** | New-syntax export only | Classic and new-syntax exports, detected per object; delta files |
| **Robustness** | One bad object loses the whole file | Each object parsed on its own; failures isolated, located and saved |
| **Code preservation** | Bodies copied, never checked | Statements and DB operations counted before and after; token-by-token check (CAL3009) |
| **Date constants** | Copied unchanged (often invalid in AL) | Order detected per file, rewritten to `yyyymmddD`, each one reported |
| **Object references** | IDs, or quoted numbers that don't compile | Names from a symbol table; valid IDs when unknown |
| **Filters and relations** | Classic syntax copied | Parsed and rewritten (TableRelation, CalcFormula, views, links, query filters) |
| **Naming** | Unnamed controls; names depend on processing order | Unique, deterministic names; implicit data item names kept |
| **Silent loss** | Unknown triggers, disabled keys and properties dropped | Nothing dropped: kept, commented with a reason, or saved as an artifact |
| **Diagnostics** | A few messages; one error stops a file | About 50 coded diagnostics (CAL0xxx–CAL9xxx), each with object, member, line, construct, reason and suggested fix |
| **Report** | Console text | `report.json`: per-object status, diagnostics and metrics |
| **Delta files** | Code changes commented out | Additions become `tableextension`/`pageextension` members; code that only adds lines becomes an extension trigger when moving it can't change behaviour |
| **Encoding** | OEM via Windows `user32` (Windows only) | BOM / UTF-8 / code-page detection, cross-platform |
| **Platform** | .NET Framework 4.7.2, Windows | .NET 10: Windows, Linux, macOS |
| **Workflow** | `convert` only | `convert`, `analyze`, `validate`, `inspect`, `compare`; exit codes for CI |
| **Verification** | None | `compare` command to measure differences against any reference output |
| **Tests** | — | 145 automated tests, approved-output snapshots, deterministic output |
| **Extras** | — | Enum objects, .NET package with unified framework versions, XLIFF per language, RDLC/Word layouts extracted |

### All advantages of Cal2AL at a glance

**Correctness**
- All 638,420 statements across both test databases carried over, with matching counts.
- Token-by-token verification of every code body.
- Locale-aware date constants.
- Name-based references.
- Valid AL filters.
- Unique control names.

**Robustness**
- Accepts classic and new-syntax exports, and delta files.
- One broken object never affects the others.
- Parse errors in code still produce AL, with the code kept and the error reported.

**Transparency**
- About 50 diagnostic codes, each with location, reason and suggested fix.
- A JSON report.
- Migration artifacts for everything that cannot be converted.

**Productivity**
- 5,000 objects converted in about 18 seconds.
- Filters by object type and ID range.
- Configurable output structure and file names.
- Dry-run `analyze` and parse-only `validate`.
- `inspect` for a single object.
- Exit codes for CI pipelines.

**Migration support**
- Table and page extensions generated from deltas.
- Enum objects, .NET package and XLIFF translation files.
- Report layouts extracted.
- `compare` against Txt2Al or any other reference.

**Maintainability**
- Clean architecture: Parser, Converter, Generator, Diagnostics, CLI, with dependency injection.
- .NET 10, cross-platform.
- 145 tests.

---

## Honest limitations

- **Not yet compiled with Microsoft's AL compiler.** Cal2AL checks its own output, but compile the generated project before trusting it.
- **Txt2Al was tested under Mono and on classic exports.** A Windows run on new-syntax exports is the next comparison to make.
- **Some things have no AL equivalent:** Forms, Dataports, MenuSuites, Automation and year-0 dates are reported and saved, not converted.
- **Translation unit IDs differ from the compiler's.** Generated XLIFF files have to be merged with the compiler's `.g.xlf` after the first build.
- **`app.json` is not generated.**

Details: [known limitations](known-limitations.md), [command-line reference](command-line-reference.md).

---

## Conclusion

Txt2Al converts the shape of an object and leaves the rest to you. On a real NAV 2016 database it either converts nothing (one bad object) or produces files full of references, filters, names and dates that don't compile.

Cal2AL converts and **proves** it did:

- every statement is accounted for;
- every problem has a code, a line and a fix;
- everything it cannot convert is kept, not lost.

For a migration team, that turns "fix thousands of compile errors" into "work through a list of about 150 well-described items".

```powershell
cal2al convert --source C:\Export --target C:\AL\MyApp
```
