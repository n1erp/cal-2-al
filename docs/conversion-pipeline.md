# Conversion pipeline

```
source (file or folder)
  │  1. Discover   *.txt / *.delta, recursive, sorted by path
  │  2. Read       bytes → text: UTF-8 BOM / UTF-16 BOM / strict UTF-8 / fallback code page (default 850)
  │  3. Split      at lines starting with "OBJECT " — each object is parsed on its own
  │  4. Parse      TXT structure → CalObject (properties, sections, elements, triggers, CODE)
  │                failure → FailedObject (CAL0101, line/column) + artifact; other objects continue
  │  5. Symbols    names, table fields and page source tables of all parsed objects
  │  6. Convert    one object at a time, ordered by type then ID
  │                ├─ declarations prepared (names to quote in code)
  │                ├─ properties  (rule table per scope → AL property / label / diagnostic)
  │                ├─ structure   (fields, keys, layout, actions, dataset, schema, elements, changes)
  │                ├─ triggers    (signature from TriggerMap; no AL equivalent → local procedure + CAL1002)
  │                └─ code bodies (see below)
  │  7. Render     AlObject → text (AlWriter, 4-space indent, CRLF, UTF-8 BOM)
  │  8. Write      .al files, layouts/, enum objects, DotNet.Package.al, Translations/*.xlf,
  │                .cal2al/artifacts/*, .cal2al/report.json
  ▼
target folder
```

## Code bodies

Every trigger and procedure body goes through `BodyConverter`:

1. **Tokenise** (`CodeLexer`, C/AL mode). Tokens keep their exact text, including whitespace and comments, so the body can be rebuilt character for character.
2. **Parse** (`CodeParser`, recursive descent) into a syntax tree. The tree is used for metrics (statements, database operations) and for rule checks (`CodeRules`: unsupported functions, classic FORM/DATAPORT references, Automation CREATE, File methods, …). If the body cannot be parsed, CAL0110 is reported and the token stream is still converted — code is never dropped because of a parse error.
3. **Rewrite** only what must change:
   * `{ … }` comments → `/* … */` (a `*/` inside is split and reported, CAL3014);
   * date constants `MMDDYY[YY]D` → `yyyymmddD` (CAL3017);
   * identifiers that are AL keywords and were declared in the object → quoted (CAL3007).
4. **Verify** (`VerifyCodePreservation`, on by default): the output is lexed as AL and compared token by token with the input after normalising the intended rewrites (comments ignored, dates compared by value, quotes on identifiers ignored). Any other difference is an error (CAL3009). The output is parsed again and its statement and database-operation counts are stored next to the input counts in the report.

## Delta files

`ModificationConverter` turns `OBJECT Modification` into a `tableextension` or `pageextension`:

| Change | Result |
|---|---|
| Insertion of fields / keys | `fields` / `keys` (extension keys renamed `<Ext>N` to avoid clashing with base `KeyN`) |
| Insertion of controls / actions | `addafter(anchor)` / `addfirst(area)` / `addlast(area)` |
| Insertion of variables / procedures | extension globals / procedures |
| Property modification allowed in extensions (Caption, Visible, …) | `modify(element) { … }` or object property |
| Code modification that only **adds** lines before/after an unchanged original | extension trigger: tables `OnBefore<X>`/`On<X>`, fields and page controls `OnBeforeValidate`/`OnAfterValidate`, actions `OnBeforeAction`/`OnAfterAction`, page triggers the extension trigger of the same name — only if moving the lines cannot change behaviour (no `EXIT` in the original when appending, no `EXIT` in prepended lines, no shared local variables, no result value) — CAL5004 |
| Anything else (changed lines, deletions, moves, unsupported properties, deltas of reports/codeunits/…) | CAL5001/5002/5003 error + the change block verbatim in `.cal2al/artifacts/<Target>.<Type>.delta.txt` |

## Migration artifacts

Written to `<target>/.cal2al/artifacts/` whenever something could not be converted: unsupported object types (Form, Dataport, MenuSuite, classic report sections), objects that failed to parse, converter failures (CAL1012) and delta changes. Each artifact is the original text, so nothing is lost.
