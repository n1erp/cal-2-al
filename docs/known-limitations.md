# Known limitations

These are known gaps. Each produces a diagnostic when it happens — none is silent.

## Comparison with Txt2Al

Cal2AL and Txt2Al were compared on two complete NAV 2016 (fr-FR) databases.
Txt2Al ran under Mono on Linux with shims for two Windows API calls ([`tools/txt2al-on-linux`](../tools/txt2al-on-linux/README.md)),
not on Windows, and on classic exports, which Txt2Al does not officially support. A comparison on new-syntax exports
on Windows (`tools/New-Txt2AlReference.ps1`) has not been done.

## Not compiled with the AL compiler

Cal2AL checks its own output (re-lexing, re-parsing, token comparison) but does not run `alc`. Compile the generated project to catch semantic errors such as:

* references to objects or fields outside the converted set (kept by ID or name, CAL4001);
* code that uses features unavailable in the target (reported as CAL3001/3003/3005/3010/3011, but kept so you can see it);
* page extensions whose added code uses globals of the base page (base page variables are not visible from an extension; Cal2AL cannot detect this without the base page).

## Object and property coverage

* Forms, Dataports, MenuSuites and reports with classic sections are not converted (artifact only). Transform forms to pages and reports to RDLC in NAV first.
* Report extensions are not generated; deltas of reports, codeunits, XMLports and queries go to the artifact (CAL5003).
* Delta anchors without a name (`Control 12`) become `Control12`/`Action12` (CAL5005); verify against the base object's AL names.
* Delta property changes are limited to properties AL allows in extensions (conservative list in `ModificationConverter`); others → CAL5002.
* `app.json` is not generated (Txt2Al does not generate it either). Create it with `AL: Go!` and copy the files in.
* Generated XLIFF files use C/AL-based unit IDs, not the hashed IDs of the compiler's `.g.xlf`; merge them with a tool such as XLIFF Sync after the first compile.
* Enum captions in languages other than ENU are not added to the XLIFF files.
* A field name containing `"` is renamed (`"` → `'`, CAL1013) because AL identifiers cannot contain double quotes.

## Code

* The C/AL parser is used for analysis; when it cannot parse a body (CAL0110) the rules that need the syntax tree (e.g. statement counts) are skipped for that body, but the tokens are still converted and verified.
* Two-digit years are read with pivot 30 (`00–29` → 20xx, `30–99` → 19xx) and reported; check them.
* `WITH` statements are kept (AL still accepts them with a warning on older runtimes; implicit `with` is an error from runtime 12 in some contexts) and reported (CAL3004/CAL3015).
* Date order is detected per file (`--date-order Auto`). A file that mixes day-first and month-first constants gets the majority order and a CAL0005 warning; a file with only ambiguous constants and no telling object dates is read month-first.
* `01010000D` (year 0, used by some NAV code as a minimum date) is not a valid calendar date, so Cal2AL keeps it unchanged and reports an error (CAL3017); decide how the code should express it.
