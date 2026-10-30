# Architecture

This document has two parts:

1. **The original Txt2Al**: how Microsoft's tool works, based on reading the decompiled source that was committed in the first commit of this repository (`chore: bring the Txt2Al beast out of the black box`: folders `Txt2Al/`, `Txt2AlConverter/`, `Microsoft.Dynamics.Nav.Model*/`, `Microsoft.Dynamics.Nav.CodeAnalysis/`). That source was removed when Cal2AL replaced it; check out the first commit to follow the file references below. Nothing here is guessed from documentation.
2. **Cal2AL**: how the new tool is built, and why it differs where it does.

---

## Part 1 — The original Txt2Al

### 1.1 What the reference source is

| Fact | Evidence |
|---|---|
| Product version 14.0.45365.0 (Business Central 14, "Spring 2019" runtime 3.2 is `RuntimeVersion.Latest`) | `Txt2Al/Properties/AssemblyInfo.cs`, `Microsoft.Dynamics.Nav.CodeAnalysis/.../RuntimeVersion.cs` |
| Targets .NET Framework 4.7.2, 32-bit preferred | `Txt2Al/Txt2Al.csproj` (`net472`, `Prefer32Bit`) |
| The code is **decompiled**, not original source | `GenerateAssemblyInfo=False`, compiler-generated patterns such as `IL_0401:` labels in `TxtFileTokenizer.cs`, `string.Concat(str1:…, str0:…)` in `AlUtilities.cs` |
| Copyright: `© 2018 Microsoft. All rights reserved.` | every `Properties/AssemblyInfo.cs` |
| No license file and no redistribution grant | no `LICENSE`, `NOTICE` or license header anywhere in the tree |

**Consequence for Cal2AL.** "All rights reserved" with no license means we have no right to copy or redistribute this code. Cal2AL therefore contains **no code taken from the reference**. The reference is used only to learn *behaviour* (what Txt2Al does to a given input), and Cal2AL implements that behaviour independently. Facts about the AL language itself (property names, trigger signatures, keyword lists) are language facts and are re-derived, not copied.

The reference solution `cal-2-al-code.sln` contains 18 projects. Only five matter for conversion; the rest are dependencies the decompiler pulled in:

| Project | Role in conversion |
|---|---|
| `Txt2Al` | Console entry point and command-line handling |
| `Txt2AlConverter` | All TXT→AL conversion logic (the "exporters") |
| `Microsoft.Dynamics.Nav.Model` | TXT tokenizer, TXT importer, generic element model, metadata tables |
| `Microsoft.Dynamics.Nav.Model.Parser` | A C/AL scanner (used only to find `{ }` comments in code) |
| `Microsoft.Dynamics.Nav.CodeAnalysis` | AL compiler: trigger definitions, property kinds, AL formatter, XLIFF writers |
| `Microsoft.Dynamics.Nav.Model.TypeSystem` | Multi-language text helpers, OEM encoding |
| `CommandLine` | The legacy CommandLineParser 1.9 library |
| `Microsoft.CodeAnalysis*`, `Mono.Cecil`, `Newtonsoft.Json`, `System.*` | Third-party/runtime libraries |

### 1.2 Entry point and command line

`Txt2Al/Txt2Al/Program.cs` → `Main` parses `AlExporterCommandLineOptions` with `Parser.Default.ParseArgumentsStrict`. On a parse error it prints usage and exits **-1**; on success it calls `ConvertFilesFromDirectory` and exits **0** — *even when conversion errors occurred*. Only an exception escaping the whole run gives -1.

Options (`AlExporterCommandLineOptions.cs`):

| Option | Default | Effect |
|---|---|---|
| `--source` (required) | | Folder with `.txt`/`.delta` files, searched **recursively** |
| `--target` (required) | | Output folder; the source sub-folder structure is mirrored |
| `--rename` | true | Name files by pattern instead of reusing the TXT file name |
| `--type` | All | Only convert one object type (Codeunit, Table, Page, Report, Query, XmlPort) |
| `--extensionStartId` | 70000000 | First ID for generated `tableextension`/`pageextension` objects |
| `--stacktrace` | false | Print stack traces of errors |
| `--fallbackTypeNameResolution` | false | Turn `Table18`-style unresolved references into plain IDs (`18`) |
| `--injectDotNetAddIns`, `--dotNetAddInsPackage` | | Add standard/custom .NET add-in declarations to `dotnet.al` |
| `--multithreaded` | false | Use 4 threads (makes extension IDs and XLIFF order non-deterministic) |
| `--dotNetTypePrefix` | "" | Prefix for generated .NET type aliases |
| `--translationFormat` | Xliff | `Xliff` or `Lcg` translation files |
| `--addLegacyTranslationInfo` | false | Add legacy C/SIDE IDs to translation units |
| `--runtime` | latest (3.2) | Target AL runtime; changes the `[Scope]` attribute it emits |
| `--objectFileNamePattern` | `{name}.{type}` | File name pattern for objects |
| `--extensionObjectFileNamePattern` | `{name}.{type}-{targetName}` | File name pattern for extension objects |
| `--format` | true | Run the AL formatter over the output folder |
| `--dataClassificationDefaulting` | false | Declared, but **never passed on**: `Program.cs` hard-codes `dataClassificationDefauling: false` |
| `--tableDataOnly` | false | Only fields and keys for tables (also suppresses code in all objects) |

### 1.3 The pipeline

`Program.ConvertFilesFromDirectory`:

```
enumerate *.txt and *.delta (recursive)
   │
   ├─ manifest.xml found in source root? ─► convert to app.json (NavAppManifest)
   │
   ▼
for each file (1 thread, or 4 with --multithreaded):
   TxtImporter.ImportFromStream  ──►  List<ApplicationObject>   (GenericElement trees)
   filter by --type
   │
   ▼
for each object:
   AlExporter.ExportObjectToString ──► <name>.<Type>.al
   RDLDATA present?   ──► <name>.rdlc
   WORDLAYOUT present ──► <name>.docx  (base64-decoded)
   │
   ▼
after all objects:
   dotnet.al          (collected .NET type declarations)
   enums.al           (collected from extensible Option fields)
   translation.<culture>.xlf   (collected labels)
   AL formatter over every *.al in the target folder (in chunks of 100 files)
   print "Total Time" and "Errors: n"
```

There is no summary per object type, no report file, and no distinction between "converted cleanly" and "converted with commented-out code".

### 1.4 Reading TXT files

**Encoding.** `TxtImportOptions.Encoding` defaults to `OemEncoding.Instance`, which builds its mapping by P/Invoking `user32!OemToCharBuffA`. Decoding therefore depends on the Windows OEM code page of the machine running the tool, and the tool cannot run outside Windows.

**Tokenizer** (`Model/IO/Txt/TxtFileTokenizer.cs`). A hand-written character scanner over a 32 KB buffer. It produces punctuation tokens, numbers, strings, quoted identifiers and keywords (`Token.cs`: `OBJECT`, `PROPERTIES`, `CODE`, `PROCEDURE`, `VAR`, `TEXTCONST`, `RDLDATA`, `WORDLAYOUT`, …). Options switch whitespace/line-break skipping and string processing on and off while parsing different parts of the file. It tracks line number and indentation, which the importer uses to find where a code block ends.

**Importer** (`Model/IO/Txt/TxtImporter.cs`). A recursive-descent parser that is *driven by metadata tables* rather than by grammar code:

* `ElementRelationExport.Definitions` (332 rows) says which sections (`FIELDS`, `KEYS`, `CONTROLS`, `DATASET`, `ELEMENTS`, …) and which triggers each element type may contain.
* `PropertyUsage.Definitions` (832 rows) says which property names each element type accepts and their data type.
* `InlineTemplate.Definitions` (24 rows) says which leading positional values a collection item has (for a table field: `FieldNo; Enabled; Name; DataType`; for a page control: `Id; Level; ControlType`; for a report data item: `Id; Indentation; DataItemType; Name`).

The result is a tree of `GenericElement` nodes, each with a property bag keyed by `PropertyType` and, for triggers and procedures, a list of raw code lines.

Behaviour that matters for conversion quality:

* **Object types.** `FORM` and `DATAPORT` objects throw "not supported". `MENUSUITE` objects are parsed and later **silently skipped** (`AlExporter.IsSupportedObjectType`).
* **Code is not parsed.** `ParseCodelines` collects the text of each line between `BEGIN` and the matching `END;` (found by indentation) and strips the indentation. Nothing checks that the code is valid.
* **Variables and signatures are parsed** into elements: name, `@id`, `Temporary`, `ARRAY [..] OF`, data type, length, subtype (`Record 18` or `Record Customer`), `TextConst` values, DotNet assembly strings and flags (`RUNONCLIENT`, `WITHEVENTS`, …), `INDATASET`, `SECURITYFILTERING`.
* **Unknown property names** abort the object with `TxtImportException`.
* **Obsolete properties and triggers** (7 properties, a few report triggers) are dropped. The importer calls `Warning(...)`, but `Txt2Al` never sets `WarningAction`, so nothing is shown.
* **One bad object loses the whole file.** Without a `WarningAction`, `ImportFromStream` re-throws the first `TxtImportException`. The list built so far is never returned, so every object in that file is lost, including the valid ones before the error. Txt2Al then reports one error for the file.

### 1.5 Converting objects

`Txt2AlConverter/.../AlExporter.cs` picks one content exporter per object type, wrapped in an `ApplicationObjectExporter` that writes the header (`table 18 Customer`) and the braces.

Each content exporter is a **chain of small exporters** (a decorator/pipeline pattern). The codeunit chain, for example, is:

```
ElementWithDocumentationExporter     // "// line" for each Documentation() line
 └─ ElementWithPropertiesExporter    // object properties via CodeunitPropertiesConverter
     └─ ElementWithTriggersExporter  // OnRun
         └─ ElementWithVariablesExporter   // global var section
             └─ ElementWithProceduresExporter
                 └─ NullExporter
```

Tables, pages, reports, queries and XMLports add an `…SectionsExporter` (fields/keys/fieldgroups, layout/actions, dataset/requestpage/labels, elements, schema). These use an `ExporterLocator` (a string-keyed service map) to choose the exporter for each child (`field`, `group`, `repeater`, `part`, `action`, `dataitem`, `column`, `tableelement`, …).

**Nesting.** C/AL stores page controls, actions, report data items, query elements and XMLport nodes as *flat lists* with an indentation level. `ElementTreeNode.GetElementsForLevel` rebuilds the tree from the `Level`/`Indentation` property.

**Properties** go through an `IPropertiesConverter` per object type (`PropertiesConverter` + `Table…`, `Page…`, `Report…`, `Query…`, `XmlPort…`, `Codeunit…PropertiesConverter`). Each converter decides, per property:

* *ignored* (e.g. `Id`, `Name`, `Level`, `FieldNo`, `DataType`, `ControlType`, `SourceExpr` — these become part of the header such as `field(1; "No."; Code[20])`),
* *renamed* (`AutoFormatExpr`→`AutoFormatExpression`, `OptionString`→`OptionMembers`, `ReqFilterFields`→`RequestFilterFields`, `DataItemLinkType`→`SqlJoinType`, `Temporary`→`UseTemporary`, `FreezeColumnID`→`FreezeColumn`, …),
* *value-converted* (`Yes/No`→`true/false`, `Code20`→`Code[20]`, `Page21`→`21`, `ContentArea`→`content`, `ActionItems`→`processing`, `RecordLinks`→`Links`, `OrderBy` reformatted, …),
* or *commented out* with an explanation (`//The property 'X' can only be set if …`). The property survives only as a comment.

Multi-language properties (`CaptionML`, `ToolTipML`, `OptionCaptionML`, …) become their single-language AL counterpart. The ENU value (or the first value) is kept, `@@@=` becomes `Comment=`, `@@@={Locked}` becomes `Locked=true`, and all language values go to the translation extractor.

`TextConst` variables become `Label` variables the same way.

### 1.6 Converting code — the key finding

`ProcedureExporter.ExportFunctionBody` writes the code **line by line, unchanged**. The only transformation applied to code bodies is in `ReplaceCommentsAndExportBody`: it runs the C/AL scanner from `Model.Parser`, finds `{ … }` comments and rewrites them as `/* … */`.

There is no expression rewriting, no handling of removed functions, and no check that the code compiles in AL. This works only because Txt2Al expects its input to come from `finsql.exe Command=ExportToNewSyntax`. That export already writes object references by name and property values in AL-style syntax. The tool even says so: `FlatCollectionInsertionExporter` throws *"The delta file was not generated with ExportToNewSyntax flag."*

Evidence that classic exports are not handled:

* `TablePropertiesConverter` passes `TableRelation` and `CalcFormula` through verbatim (`FormatMultiLineProperty`). A classic value such as `"Cust. Ledger Entry".Amount WHERE (Customer No.=FIELD(No.))` is emitted as-is and does not compile.
* `PagePropertiesConverter` writes `SourceTable` through `QuoteIdentifierIfNeeded`, so a classic `Table18` comes out as `SourceTable = Table18;`, which is invalid AL.
* `AlTableRelationCalcFormula` contains a quoting routine for classic formulas, but **nothing calls it** (dead code). Its `ConstFunction` also maps `No`→`true` and `Yes`→`false`, which is backwards.

Procedure headers *are* rebuilt from the parsed elements. Txt2Al writes attributes (`[EventSubscriber(...)]`, `[IntegrationEvent(...)]`, `[BusinessEvent(...)]`, `[External]`→`[Scope('Cloud')]`, `[Internal]`/no attribute→`[Scope('OnPrem')]` from runtime 4.0, `[Scope('Internal')]` before), `local`, parameters (`var`, types via the type resolver), and the return value. Trigger signatures come from `TriggerDefinitions` in the AL compiler (e.g. page `OnFindRecord(Which: Text): Boolean`, control `OnLookup(var Text: Text): Boolean`).

### 1.7 Silent loss and comment substitution in the original

These are the places where Txt2Al drops code or replaces it with a comment. Cal2AL must not do either.

| Behaviour | Where |
|---|---|
| A trigger with no AL equivalent is silently dropped. `GetTriggerTypeInfo` returns `null` and the trigger is skipped with no message | `ElementWithTriggersExporter.ExportTrigger` |
| MenuSuite objects are silently skipped | `AlExporter.IsSupportedObjectType` |
| Obsolete properties are silently dropped at import | `TxtImporter.ParsePropertiesOrTriggers` |
| One malformed object discards every object in the same file | `TxtImporter.ImportFromStream` + `Program.ExtractApplicationObjectsFromFile` |
| Disabled keys (`Enabled=No`) are dropped | `TableKeyExporter.IsKeyElementDisabled` |
| `--tableDataOnly` drops all triggers, procedures and variables of every object | `Element*Exporter.IsTrigger/IsProcedure`, `ElementWithVariablesExporter` |
| Properties that fail a rule are written as `//Property = value;` comments | `ElementWithPropertiesExporter.WriteProperty`, `*PropertiesConverter.TryCommentOutProperty` |
| DateTime `InitValue` is commented out ("not supported") | `TablePropertiesConverter` |
| .NET event triggers whose owner cannot be found are emitted fully commented out | `UnresolvedDotNetEventsExporter` |
| In `.delta` files, code modifications, trigger changes and global-variable property changes are emitted as commented-out "ORIGINAL CODE / MODIFIED CODE" blocks with an "Unsupported feature" comment | `CommentedCodeDeltaExporter`, `ChangeContentExporter` |
| Delta files can only target tables and pages; other targets are skipped | `AlExporter.IsChangeTypeSupported` |
| A key gets a generated name (`Key1`, `Key2`, …) because C/AL keys have no names | `TableKeyExporter` |
| Controls and actions without a name get `Control<id>` / `Action<id>` | `AlUtilities.GetElementName` |

### 1.8 Writing output

`AlWriter` is an indentation-aware text writer (`PushIndent`/`PopIndent`, `WriteSection` writes `name`, `{`, content, `}`), with a `PendingEmptyLine` flag that the exporters use to put blank lines between sections. All exporters write into a `MemoryStream` with UTF-16. The CLI decodes it again and saves it with `File.WriteAllText` (UTF-8 without a BOM).

File names come from `BaseFileNameExporter`: the pattern tokens `{name}`, `{type}`, `{id}`, `{targetName}`, `{targetId}`. The name is stripped to letters and digits, and a name collision gets `.<id>` appended.

The optional formatter (`Formatter.cs`) loads each output file into an in-memory Roslyn-style AL workspace and calls the AL compiler's formatter.

### 1.9 Version-specific behaviour

* `--runtime` only affects the scope attribute: `[Scope('OnPrem'|'Cloud')]` for runtime ≥ 4.0, otherwise `[Scope('Internal'|'Extension')]` (`AlUtilities.GetCompilationScopeAttribute`).
* Extensible Option fields (`Extensible=Yes`, plus `EnumTypeName`/`EnumTypeId`, a NAV 2018/BC14 feature) become `enum` types in `enums.al`.
* .NET variables are collected into a `dotnet { assembly(...) { type(...) } }` package.
* `AccessByPermission`, `ApplicationArea` (`#All` → `All`), `APIVersion` and the API page properties are handled. API-only properties on non-API pages are commented out.

### 1.10 Diagnostics in the original

* Errors are plain strings in a `ConcurrentQueue<Exception>`, printed in red at the end, plus the count.
* Warnings (`WarningExporter.DisplayConsoleWarning`) go to stderr while converting, are not counted, and are not tied to a file or line.
* The exit code is 0 unless the whole run throws.

---

## Part 2 — Cal2AL

### 2.1 Goals that drive the design

1. **Never lose code silently.** Every trigger, procedure, statement and property is either converted, or kept *as executable code* with a diagnostic, or (only when it genuinely cannot exist in AL) saved verbatim in a migration artifact with a structured diagnostic. A conversion that dropped something is reported as failed or partial, never as success.
2. **Accept both export dialects**: classic `finsql` TXT exports *and* `ExportToNewSyntax` exports. Classic exports are what most projects actually have.
3. **Explain itself**: per-object results, JSON reports, and diagnostics with object, member, line, construct, reason and suggested fix.
4. **Clean room**: no code copied from the reference (see 1.1).

### 2.2 Solution layout

```
cal2al/                    CLI (the original empty console app; assembly name "cal2al")
src/Cal2AL.Diagnostics/    Diagnostic model, codes, report model, JSON export
src/Cal2AL.Core/           Domain model of NAV objects (CalObject, CalElement, CalProperty, …)
src/Cal2AL.Parser/         TXT lexer + object parser; C/AL code lexer + statement/expression parser
src/Cal2AL.Converter/      Object conversion, property rules, code rewriting, symbol table
src/Cal2AL.Generator/      AL source writer, file naming, XLIFF writer
tests/Cal2AL.Tests/        xUnit tests (unit, integration, comparison harness)
tests/Fixtures/            TXT input fixtures and expected AL
```

Dependencies only point downwards: `CLI → Converter → (Parser, Generator) → Core → Diagnostics`.

### 2.3 How it differs from Txt2Al

| Concern | Txt2Al | Cal2AL |
|---|---|---|
| Input dialects | ExportToNewSyntax only (in practice) | Classic and new-syntax; dialect detected per object |
| Encoding | Windows OEM code page via user32 | UTF-8 (BOM or strict) auto-detection, fallback code page configurable (`--encoding`, default 850) |
| Platform | Windows, .NET Framework 4.7.2 | Any OS, .NET 10 |
| Bad object in a file | Whole file lost | Parser resynchronises at the next `OBJECT` line; only the bad object fails |
| Code | Copied as text | Tokenised and parsed into an AST; rewritten at token level; unconvertible constructs reported with position |
| Object references | Must already be names | Symbol table built from all input objects resolves `Record 18`, `Table18`, `Page21` to names when the object is in the input, else to IDs (valid AL) |
| Classic `TableRelation`/`CalcFormula`/links/views | Emitted verbatim (invalid AL) | Parsed and rewritten to AL syntax |
| Unknown triggers | Silently dropped | Kept as a procedure with the same body + error diagnostic, or saved to an artifact |
| Unsupported properties | Commented out | Kept if valid in AL; otherwise reported with the original value in the report |
| MenuSuites, Forms, Dataports | Skipped or rejected | Reported per object with a migration artifact (original text) |
| Reporting | Error strings and a count | Structured diagnostics, per-object status, console summary, JSON report, meaningful exit codes |

### 2.4 Pipeline

```
discover files ─► decode ─► TXT lexer ─► object parser ─► CalObject model
                                                         │
                         symbol table (all objects) ◄────┘
                                                         │
                     per object: ObjectConverter ─► AL model (AlObject…) ─► AlWriter ─► .al
                                    │  property rules
                                    │  trigger mapping
                                    │  code: C/AL lexer ─► parser (AST, validation)
                                    │        ─► token rewriter ─► AL body text
                                    ▼
                               Diagnostics ─► ConversionReport ─► console / JSON
```

The detailed stage-by-stage description is in [conversion-pipeline.md](conversion-pipeline.md).

### 2.5 Additional findings about Txt2Al (confirmed while building Cal2AL)

* **Date constants are copied verbatim.** C/AL writes date constants as `MMDDYY[YY]D` ([C/AL Constants](https://learn.microsoft.com/en-us/dynamics-nav/c-al-constants)); AL writes `yyyymmddD` ([AL Date data type](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/developer/methods-auto/date/date-data-type)). Because Txt2Al copies code bodies unchanged, `010125D` survives in its output and means something else (or nothing) in AL. Cal2AL rewrites every date constant (pivot year 30 for two-digit years, `--date-order` for exports that used day-first dates) and reports each one (CAL3017). Time constants use the same format in both languages and are kept.
* **Control/action names depend on processing order.** Txt2Al keeps the set of already-used generated names in a *static* collection that is not reset between objects, so the same page can get different names depending on which objects were converted before it. Cal2AL scopes names to the object (`ObjectContext.ReserveName`) and converts objects in a fixed order (type, then ID), so output is deterministic (tested by `Output_is_deterministic`).

### 2.6 Project map (implementation)

| Area | Main types |
|---|---|
| Parsing | `TxtFileParser`, `TxtObjectParser`, `TextDecoder`, `CodeLexer`, `CodeParser`, `DateLiteral` |
| Conversion | `IObjectConverter` + `TableConverter`, `PageConverter` (`PageBuilder`), `ReportConverter` (`RequestPageConverter`), `CodeunitConverter`, `XmlPortConverter`, `QueryConverter`, `ModificationConverter`; shared `MemberConverter`, `BodyConverter`, `PropertyConverter`, `FilterExpressionConverter`, `LabelConverter` |
| Run orchestration | `ConversionEngine` (`IConversionEngine`), `OutputLayout`, `IOutputSink` (`FileSystemOutputSink`, `InMemoryOutputSink`), `PackageWriters` (enums, .NET package, XLIFF) |
| Comparison | `AlOutlineReader`, `AlComparer` |
| Generation | `AlObject` model, `AlWriter`, `AlSyntax` |
| Diagnostics | `DiagnosticDescriptors`, `Diagnostic`, `ConversionReport` |
| CLI | `cal2al/` (`Program`, `CommandHandlers`, `CliOptions`, `ConsoleReporter`, `ExitCodes`) |

Dependency injection: `services.AddCal2AL()` registers every `IObjectConverter` and the `IConversionEngine`; the CLI resolves the engine from the container. Converters are used one object at a time (they may keep per-object state), which also keeps generated IDs deterministic.
