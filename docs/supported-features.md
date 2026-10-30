## Input

| Feature | Status |
|---|---|
| Classic C/SIDE TXT export (`finsql ExportObjects`) | Tested (fixtures `classic/`) |
| New-syntax TXT export (`ExportToNewSyntax`) | Tested (`newsyntax/TAB50050`) — dialect detected per object |
| Delta files (`.DELTA`, `OBJECT Modification`) | Tested (`delta/`) |
| Several objects per file; a broken object does not affect the others | Tested (`malformed/MULTI.TXT`) |
| Encodings: UTF-8 (with/without BOM), UTF-16 BOM, OEM code page fallback (850 default, `--encoding`) | Tested (`encoding/`, `DateLiteralAndDecoderTests`) |
| Folder search (recursive, `.txt`/`.delta`) | Tested |

## Objects

| Object | Converted parts | Status |
|---|---|---|
| Table | properties, fields (types, options, extensible options → enum objects), field triggers, keys (disabled keys kept with `Enabled = false`), field groups, triggers, code | Tested |
| Page | properties, layout (areas, groups/repeater/cuegroup/fixed/grid, fields, labels, parts, system parts, chart parts, user controls), actions (areas, groups, actions, separators, actions outside a container), control/action triggers, control add-in events placed in the `usercontrol`, code | Tested |
| Report (RDLC/Word) | properties, dataset tree (data items, columns, links, views), request page, labels, triggers, RDLC and Word layouts extracted to `layouts/` | Tested (RDLC); Word layout extraction unit-untested |
| Codeunit | properties (incl. TestRunner, Subtype), OnRun, test runner triggers, code | Tested |
| XMLport | properties, schema (text/table/field elements and attributes, XmlName when renamed, TextType BigText), node triggers, request page | Tested |
| Query | properties, data items, columns, filters, links, join types, OrderBy, method/totals | Tested |
| Table/page delta | see [conversion pipeline](conversion-pipeline.md#delta-files) | Tested |
| Form, Dataport, MenuSuite, classic report sections | Not convertible: CAL1001 + artifact | Tested |

## Values and properties

| Feature | Status |
|---|---|
| Multi-language captions → labels with `Comment`/`Locked`; non-ENU texts → XLIFF per language | Tested |
| TextConst → `Label` | Tested |
| TableRelation (incl. `IF … ELSE`), CalcFormula, table views, links, query links/filters, OrderBy — classic and new syntax | Tested |
| `CONST(Yes/No)` → `const(true/false)` for Boolean fields (or unknown tables, with CAL2008) | Tested |
| Object references by ID resolved to names when the object is in the converted set (otherwise ID kept, CAL4001 once per object) | Tested |
| Permissions | Tested |
| Unsupported / inert properties reported (CAL2001/CAL2007) instead of silently dropped | Tested |

## Code

| Feature | Status |
|---|---|
| Every statement carried over; token-by-token preservation check; statement and DB-operation counts in = out | Tested (all fixtures) |
| Brace comments → block comments (nested braces, `*/` inside) | Tested |
| Date constants rewritten to AL format; day/month order detected per file (exports follow the exporting machine's locale), century pivot 30 | Tested, incl. 2 real fr-FR databases |
| AL keywords used as identifiers quoted | Tested |
| Attributes: External, Internal, Integration, Business, EventSubscriber (target resolved), TryFunction, test attributes, handler functions | Tested (fixture COD50010) |
| DotNet variables → `DotNet.Package.al` with aliases; WithEvents → `trigger Var::Event` | Tested |
| Unsupported runtime functions (SHELL, ISSERVICETIER, CREATE, ENVIRON, …), classic FORM/DATAPORT references, Automation, File (OnPrem) reported with reason and resolution; code kept | Tested |
| Code that does not parse is still carried over (CAL0110) | Tested |

## Output and tooling

| Feature | Status |
|---|---|
| Output structure Mirror / ByType / Flat, file name patterns, collision handling (CAL9002) | Mirror tested; others unit-untested |
| JSON report with per-object status, diagnostics and metrics | Tested (engine); CLI writes it by default |
| Deterministic output | Tested |
| CLI: convert, analyze, validate, inspect, compare; verbose/quiet; exit codes | Options parsing tested; commands exercised manually |
| Comparison with reference output (equivalent / formatting / behavioural / missing), pairing renamed elements by position | Tested; used on 9,704 real objects  |
