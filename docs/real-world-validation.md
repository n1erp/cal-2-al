# Real-world validation: NAV 2016 databases, Cal2AL vs Txt2Al

Both tools were run on two complete C/SIDE exports of French NAV 2016 databases and their outputs compared with
`cal2al compare`. This page records what was measured, what was wrong in each tool, and what was fixed in Cal2AL.

## Input

| File | Size | Objects | Notes |
|---|---|---|---|
| `All_Obj_NAV2016.txt` | 154 MB | 5,028 (1,211 tables, 2,147 pages, 778 reports, 800 codeunits, 49 XMLports, 39 queries, 3 MenuSuites) | customised database, classic export, code page 850, fr-FR |
| `All_Obj_STANDARD.txt` | 136 MB | 4,676 | standard French database, classic export |

Both are **classic** exports (`finsql ExportObjects`), not `ExportToNewSyntax`. The
[Txt2Al documentation](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/developer/devenv-txt2al-tool)
requires new-syntax exports; classic exports were used because they are what most NAV databases have, and because
they show how each tool behaves on input it does not fully understand.

## How Txt2Al was run

Txt2Al 14.0 (the binaries supplied with this repository's reference) was run under Mono on Linux with two native shims
for its Windows API calls; see [`tools/txt2al-on-linux`](../tools/txt2al-on-linux/README.md). The command was the
documented one: `txt2al --source=<folder> --target=<folder> --rename`.

## Results

| | Txt2Al | Cal2AL |
|---|---|---|
| Whole export as one file | **0 objects**: stops at system table 2000000140 (`WriteProtected=Yes is not valid property for Table`) and discards the file | 5,028 / 4,676 objects processed |
| Export split into one file per object | 5,025 / 4,675 `.al` files; 4,657 / 4,530 warnings "Name … has not been correctly exported by C\SIDE" (classic export) | — |
| Objects converted (NAV2016) | — | 4,564 converted, 443 with warnings, 18 need manual work, 3 failed (MenuSuites) |
| Code statements, input → output (NAV2016 / STANDARD) | not measured by Txt2Al | 327,184 → 327,184 / 311,236 → 311,236 |
| Database operations, input → output | not measured | 40,172 → 40,172 / 37,771 → 37,771 |
| Preservation check failures (CAL3009) | — | 0 |

### Comparison (NAV2016; STANDARD is similar)

| Category | Entries |
|---|---|
| Equivalent | 288,359 |
| Formatting only | 11,465 |
| Behavioural difference | 64,447 |
| Only in Txt2Al output | 3,362 |
| Only in Cal2AL output | 4,185 |

Behavioural differences, classified by pattern (NAV2016):

| Pattern | Entries | Which output is valid AL |
|---|---|---|
| Txt2Al writes object references as quoted numbers: `Record "36"`, `Codeunit "80"` | 23,877 | Cal2AL (`Record "Sales Header"`); `"36"` names a table called 36 |
| Txt2Al copies classic filter syntax: `WHERE (Customer No.=FIELD(No.))` | 12,685 | Cal2AL (`where("Customer No." = field("No."))`) |
| Txt2Al leaves classic references: `dataitem(DataItem6710; Table15)` | 4,536 | Cal2AL (`dataitem("G/L Account"; "G/L Account")`) |
| Txt2Al emits unnamed controls: `systempart( ; Links)`, `repeater( )`, `part( ; 1028)` | 6,309 | Cal2AL (names every control) |
| Txt2Al keeps `&` from captions in names: `group("&Company")` | 3,804 | both compile; Cal2AL strips the access key |
| Txt2Al uses IDs where Cal2AL uses names: `LookupPageID = 22` | 3,224 + most `RunObject` | both valid |
| Other naming and layout-path differences | ~10,000 | reviewed by sample; see below |

Other Txt2Al defects seen in its output: date constants copied in the export's day-first order (`31129999D`), the
`Namespaces` property of PEPPOL XMLports split across lines into an invalid string, disabled keys silently dropped,
and unnamed report data items renamed `DataItem<id>` although code refers to them by table name.

## Defects found in Cal2AL and fixed

| Found | Fix | Regression test |
|---|---|---|
| 6 objects failed to parse: text constants whose strings start on the next line (`TextConst` + newline + `'@@@=…',`) | parser accepts strings on following lines | `Text_constants_whose_strings_start_on_the_next_line_are_parsed` |
| … and unnamed return values `PROCEDURE X@1() @1000 : Text[80];` | parser accepts `@id` without a name | `Unnamed_return_values_are_parsed` |
| 3 bodies did not parse: set elements with comparisons `TRUE IN [A <> B, NOT (C OR D)]` | set elements are full expressions | `Set_elements_can_be_comparisons_and_boolean_expressions` |
| 192 date constants wrong or rejected: C/SIDE writes date constants in the **exporting machine's regional order** (`31129999D` in a French export), not always `MMDDYY` | `--date-order Auto` (new default) detects the order per file (CAL0005) | `Day_first_date_constants_are_detected_and_converted`, `Date_order_falls_back_to_object_dates_then_month_first` |
| Word layouts not extracted: C/SIDE base64-encodes them in padded chunks, one per line | decode line by line | `Word_layouts_encoded_in_padded_chunks_are_decoded` |
| Query `Count` columns reported as missing DataSource | `column(Name) { Method = Count; }` | `Query_count_columns_have_no_data_source` |
| Unnamed query columns got `Column12` | named after the source field (`Entry_Point`) | snapshot |
| `OptionOrdinalValues=[0;1;2;3]` written as `0;1;2;3` (invalid) | `0, 1, 2, 3` | `Option_ordinal_value_lists_use_commas` |
| Placeholder key `{ No ; }` written as `key(Key2; )` (invalid) | not emitted, reported | `Disabled_placeholder_keys_without_fields_are_not_emitted` |
| `ProviderID=1120022` written as `Provider = "1120022"` | resolved to the part's AL name | `Provider_id_is_resolved_to_the_providing_part_name` |
| `InitValue=[ ]` (blank option member) written as `""` | `" "` | — |
| mscorlib 2.0 and 4.0 types declared twice (`CultureInfo`, `CultureInfo2`) | .NET Framework versions unified | `Framework_assembly_versions_are_unified_into_one_declaration` |
| Repeated XMLport node names fell back to `Node<line>` | `LineNo_2` with `XmlName = 'LineNo'` | — |
| Automation event triggers reported as "no WithEvents variable" | reported as Automation (CAL3005) | — |
| `compare` paired elements by name only, so a renamed data item made all its columns "missing" | second pass pairs by position | `AlComparerTests` |

## What still needs manual work (NAV2016)

| Diagnostic | Count | Meaning |
|---|---|---|
| CAL3005 Automation | 102 | Excel/COM automation (e.g. codeunit 7152); replace with Excel Buffer or .NET |
| CAL3017 invalid date | 25 | `01010000D` (year 0) is not a valid calendar date; kept unchanged for review |
| CAL1001 unsupported object | 3 | MenuSuites |
| Warnings (2,020) | | mostly .NET variables (CAL3011, on-premises only), obsolete functions (CAL3002), File variables (CAL3003) |

The generated projects have not yet been compiled with the AL compiler.
