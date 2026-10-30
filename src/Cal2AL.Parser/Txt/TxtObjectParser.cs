using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Cal2AL.Core.Model;

namespace Cal2AL.Parser.Txt;

/// <summary>
/// Parses the text of one C/AL object (from its <c>OBJECT</c> line to its closing brace) into a <see cref="CalObject"/>.
/// The parser understands both classic exports and ExportToNewSyntax exports; it does not interpret property values
/// or code, it only splits the file into its structural parts and keeps every piece of text with its position.
/// </summary>
public sealed partial class TxtObjectParser
{
    private TextCursor cursor = null!;
    private CalObjectType objectType;

    /// <summary>Parses an object. Throws <see cref="TxtParseException"/> on malformed input.</summary>
    /// <param name="objectText">Text of the object, starting with the OBJECT line.</param>
    /// <param name="sourceFile">File name, for positions and diagnostics.</param>
    /// <param name="lineOffset">Number of lines in the file before the OBJECT line.</param>
    public CalObject Parse(string objectText, string sourceFile, int lineOffset = 0)
    {
        cursor = new TextCursor(objectText, lineOffset);
        cursor.SkipWhitespace();
        var headerPosition = cursor.SourcePosition;
        cursor.ExpectWord("OBJECT");
        var header = ParseHeader(cursor.ReadLine());
        objectType = header.Type;

        var obj = new CalObject
        {
            Type = header.Type,
            Id = header.Id,
            Name = header.Name,
            SourceFile = sourceFile,
            RawText = objectText,
            Position = headerPosition,
            EndLine = lineOffset + CountLines(objectText),
            ModificationTarget = header.Type == CalObjectType.Modification ? header.Name : null,
            UnsupportedReason = GetUnsupportedReason(header.Type, objectText),
        };

        if (obj.UnsupportedReason is not null)
        {
            // Keep header and raw text only; the converter turns it into a migration artifact.
            return obj;
        }

        cursor.SkipWhitespace();
        cursor.Expect('{');
        while (true)
        {
            cursor.SkipWhitespace();
            if (cursor.AtEnd)
            {
                throw cursor.Error("Unexpected end of object; '}' expected.");
            }

            if (cursor.Peek() == '}')
            {
                cursor.Next();
                break;
            }

            if (cursor.LookingAtWord("OBJECT") && cursor.Peek(6) == '-')
            {
                cursor.Advance(6);
                cursor.Expect('-');
                cursor.ExpectWord("PROPERTIES");
                var holder = new CalElement { SectionName = "OBJECT-PROPERTIES" };
                OpenBlock();
                ParsePropertyBlock(holder);
                cursor.Expect('}');
                foreach (var p in holder.Properties)
                {
                    obj.ObjectProperties[p.Name] = p.Value;
                }

                continue;
            }

            var sectionPosition = cursor.SourcePosition;
            var word = cursor.ReadWord();
            switch (word.ToUpperInvariant())
            {
                case "":
                    throw cursor.Error($"Section name expected but found '{cursor.Describe()}'.");
                case "PROPERTIES":
                    OpenBlock();
                    ParsePropertyBlock(obj);
                    cursor.Expect('}');
                    break;
                case "CODE":
                    ParseCodeSection(obj);
                    break;
                case "RDLDATA":
                    ParseRawBlock(obj.RdlData, "END_OF_RDLDATA");
                    break;
                case "WORDLAYOUT":
                    ParseRawBlock(obj.WordLayout, "END_OF_WORDLAYOUT");
                    break;
                default:
                    obj.Sections.Add(ParseSection(word, sectionPosition));
                    break;
            }
        }

        obj.Dialect = DialectDetector.Detect(obj);
        return obj;
    }

    // ------------------------------------------------------------------------------------------------
    // Header
    // ------------------------------------------------------------------------------------------------

    internal readonly record struct ObjectHeader(CalObjectType Type, int Id, string Name);

    [GeneratedRegex(@"^\s*\[?\s*(?<type>[A-Za-z]+)\s+(?<id>\d+)\s*(?<name>.*?)\]?\s*$")]
    private static partial Regex HeaderRegex();

    [GeneratedRegex(@"^\s*Modification\s+(?<name>.*)$", RegexOptions.IgnoreCase)]
    private static partial Regex ModificationHeaderRegex();

    internal static ObjectHeader ParseHeader(string line)
    {
        var mod = ModificationHeaderRegex().Match(line);
        if (mod.Success)
        {
            var target = mod.Groups["name"].Value.Trim();
            var targetMatch = Regex.Match(target, @"\((?<type>[A-Za-z]+)\s+(?<id>\d+)\)\s*$");
            var id = targetMatch.Success ? int.Parse(targetMatch.Groups["id"].Value, CultureInfo.InvariantCulture) : 0;
            return new ObjectHeader(CalObjectType.Modification, id, target);
        }

        var m = HeaderRegex().Match(line);
        if (!m.Success)
        {
            throw new TxtParseException($"Invalid object header 'OBJECT{line}'. Expected 'OBJECT <Type> <Id> <Name>'.", 1, 1);
        }

        var type = CalObjectTypeExtensions.ParseObjectType(m.Groups["type"].Value);
        if (type == CalObjectType.Unknown)
        {
            throw new TxtParseException($"Unknown object type '{m.Groups["type"].Value}'.", 1, 8);
        }

        return new ObjectHeader(type, int.Parse(m.Groups["id"].Value, CultureInfo.InvariantCulture), m.Groups["name"].Value.Trim());
    }

    [GeneratedRegex(@"^\s{2}(DATAITEMS|SECTIONS)\s*$", RegexOptions.Multiline)]
    private static partial Regex ClassicReportRegex();

    private static string? GetUnsupportedReason(CalObjectType type, string text) => type switch
    {
        CalObjectType.Form => "Forms are a classic-client object type and do not exist in AL. Transform them to pages (NAV 2009 R2 or later) first.",
        CalObjectType.Dataport => "Dataports are a classic-client object type and do not exist in AL. Re-implement them as XMLports.",
        CalObjectType.MenuSuite => "MenuSuites do not exist in AL. Navigation in Business Central is provided by UsageCategory/ApplicationArea on pages and reports, and by role centers.",
        CalObjectType.Report when ClassicReportRegex().IsMatch(text) =>
            "This is a classic report (DATAITEMS/SECTIONS). Only RDLC reports (DATASET) can be converted. Transform it in NAV 2009 R2 or later first.",
        _ => null,
    };

    private static int CountLines(string text)
    {
        var count = 1;
        foreach (var c in text)
        {
            if (c == '\n')
            {
                count++;
            }
        }

        return count;
    }

    private void OpenBlock()
    {
        cursor.SkipWhitespace();
        cursor.Expect('{');
    }

    // ------------------------------------------------------------------------------------------------
    // Sections and items
    // ------------------------------------------------------------------------------------------------

    private CalSection ParseSection(string name, SourcePosition position)
    {
        var section = new CalSection(name, position);
        if (SectionLayouts.IsSingleNodeSection(name))
        {
            OpenBlock();
            var node = new CalElement { SectionName = name, Position = cursor.SourcePosition };
            ParseNodeBody(node);
            section.Items.Add(node);
            return section;
        }

        var columns = SectionLayouts.GetColumns(objectType, "Object", name)
            ?? throw new TxtParseException($"Section '{name}' is not valid in a {objectType} object.", position.Line, position.Column);
        OpenBlock();
        ParseItems(section, columns);
        return section;
    }

    /// <summary>Parses <c>PROPERTIES { … }</c> followed by nested sections, up to and including the closing brace.</summary>
    private void ParseNodeBody(CalElement node)
    {
        while (true)
        {
            cursor.SkipWhitespace();
            if (cursor.Peek() == '}')
            {
                cursor.Next();
                return;
            }

            var position = cursor.SourcePosition;
            var word = cursor.ReadWord();
            if (word.Length == 0)
            {
                throw cursor.Error($"Section name expected but found '{cursor.Describe()}'.");
            }

            if (word.Equals("PROPERTIES", StringComparison.OrdinalIgnoreCase))
            {
                OpenBlock();
                ParsePropertyBlock(node);
                cursor.Expect('}');
            }
            else
            {
                node.Sections.Add(ParseSection(word, position));
            }
        }
    }

    /// <summary>Parses <c>{ item } { item } … }</c>; the cursor is just after the opening brace of the section.</summary>
    private void ParseItems(CalSection section, string[] columns)
    {
        while (true)
        {
            cursor.SkipWhitespace();
            if (cursor.AtEnd)
            {
                throw cursor.Error($"Unexpected end of object inside section '{section.Name}'.");
            }

            if (cursor.Peek() == '}')
            {
                cursor.Next();
                return;
            }

            var position = cursor.SourcePosition;
            cursor.Expect('{');
            section.Items.Add(ParseItem(section.Name, columns, position));
        }
    }

    private CalElement ParseItem(string sectionName, string[] columns, SourcePosition position)
    {
        var item = new CalElement { SectionName = sectionName, Position = position };
        cursor.SkipWhitespace();
        if (cursor.LookingAtWord("PROPERTIES"))
        {
            ParseNodeBody(item);
            item.LastSourceLine = cursor.Line;
            return item;
        }

        var terminator = ';';
        for (var i = 0; i < columns.Length && terminator == ';'; i++)
        {
            item.Positional.Add(ReadValue(out terminator));
        }

        if (terminator == ';')
        {
            ParsePropertyBlock(item);
        }

        cursor.SkipWhitespace();
        cursor.Expect('}');
        item.LastSourceLine = cursor.Line;
        return item;
    }

    // ------------------------------------------------------------------------------------------------
    // Properties and triggers
    // ------------------------------------------------------------------------------------------------

    /// <summary>
    /// Parses <c>Name=Value;</c> pairs, triggers and container properties until a closing brace, which is not consumed.
    /// </summary>
    private void ParsePropertyBlock(CalNode node)
    {
        while (true)
        {
            cursor.SkipWhitespace();
            if (cursor.AtEnd)
            {
                throw cursor.Error("Unexpected end of object inside a property list.");
            }

            if (cursor.Peek() == '}')
            {
                return;
            }

            var position = cursor.SourcePosition;
            var name = ReadPropertyName();

            if (SectionLayouts.IsCodeProperty(name) || (IsTriggerName(name) && LooksLikeCodeStart()))
            {
                node.Triggers.Add(ParseTrigger(name, position));
                continue;
            }

            if (LooksLikeContainer())
            {
                node.Sections.Add(ParseContainer(position));
                continue;
            }

            var value = ReadValue(out var terminator);
            node.Properties.Add(new CalProperty(name, value, position));
            if (terminator == '}')
            {
                return;
            }
        }
    }

    private string ReadPropertyName()
    {
        var sb = new StringBuilder();
        while (true)
        {
            var c = cursor.Peek();
            if (c == '=')
            {
                cursor.Next();
                break;
            }

            if (c is '\n' or '\0' or '{' or '}' or ';')
            {
                throw cursor.Error($"Property name followed by '=' expected but found '{(sb.Length > 0 ? sb.ToString().Trim() : cursor.Describe())}'.");
            }

            sb.Append(cursor.Next());
        }

        var name = sb.ToString().Trim();
        if (name.Length == 0)
        {
            throw cursor.Error("Empty property name.");
        }

        return name;
    }

    private static bool IsTriggerName(string name) =>
        name.Contains("::", StringComparison.Ordinal)
        || (name.Length > 2 && name.StartsWith("On", StringComparison.OrdinalIgnoreCase) && char.IsUpper(name[2]));

    /// <summary>True if the value at the cursor starts with VAR or BEGIN alone on the line (a trigger body).</summary>
    private bool LooksLikeCodeStart()
    {
        var mark = cursor.Save();
        try
        {
            cursor.SkipSpaces();
            if (!cursor.TryReadWord("VAR") && !cursor.TryReadWord("BEGIN"))
            {
                return false;
            }

            cursor.SkipSpaces();
            return cursor.AtLineEnd || cursor.Peek() == '\r';
        }
        finally
        {
            cursor.Restore(mark);
        }
    }

    /// <summary>True if the value is a single word followed by a block on the next line (<c>ActionList=ACTIONS { … }</c>).</summary>
    private bool LooksLikeContainer()
    {
        var mark = cursor.Save();
        try
        {
            cursor.SkipSpaces();
            if (cursor.ReadWord().Length == 0)
            {
                return false;
            }

            cursor.SkipSpaces();
            if (!(cursor.AtLineEnd || cursor.Peek() == '\r'))
            {
                return false;
            }

            cursor.SkipWhitespace();
            return cursor.Peek() == '{';
        }
        finally
        {
            cursor.Restore(mark);
        }
    }

    private CalSection ParseContainer(SourcePosition position)
    {
        cursor.SkipSpaces();
        var containerName = cursor.ReadWord();
        var section = new CalSection(containerName, position);
        OpenBlock();

        if (SectionLayouts.IsDeclarationContainer(containerName))
        {
            ParseDeclarationContainer(section);
            return section;
        }

        var columns = SectionLayouts.GetColumns(objectType, "Container", containerName)
            ?? throw new TxtParseException($"Container '{containerName}' is not supported in a {objectType} object.", position.Line, position.Column);
        ParseItems(section, columns);
        return section;
    }

    private void ParseDeclarationContainer(CalSection section)
    {
        while (true)
        {
            cursor.SkipWhitespace();
            if (cursor.Peek() == '}')
            {
                cursor.Next();
                return;
            }

            if (cursor.Peek() == '[' || cursor.LookingAtWord("LOCAL") || cursor.LookingAtWord("PROCEDURE") || cursor.LookingAtWord("EVENT"))
            {
                section.Procedures.Add(ParseProcedure());
                continue;
            }

            var variable = ParseVariable(isParameter: false);
            section.Variables.Add(variable);
            cursor.SkipWhitespace();
            if (cursor.Peek() == ';')
            {
                cursor.Next();
            }
        }
    }

    /// <summary>
    /// Reads a property or positional value. Handles C/SIDE's bracket wrapping (<c>[ … ]</c> with <c>]]</c> escapes)
    /// and values continued on following lines. Returns the value and the terminator found (<c>;</c> or <c>}</c>);
    /// a <c>;</c> is consumed, a <c>}</c> is not.
    /// </summary>
    private string ReadValue(out char terminator)
    {
        cursor.SkipSpaces();
        var sb = new StringBuilder();

        if (cursor.Peek() == '[')
        {
            cursor.Next();
            var alignColumn = cursor.Column - 1; // characters before the first value character on the line
            while (true)
            {
                var c = cursor.Peek();
                if (c == '\0')
                {
                    throw cursor.Error("Unterminated '[' in property value.");
                }

                if (c == ']')
                {
                    if (cursor.Peek(1) == ']')
                    {
                        cursor.Advance(2);
                        sb.Append(']');
                        continue;
                    }

                    cursor.Next();
                    break;
                }

                if (c == '\r')
                {
                    cursor.Next();
                    continue;
                }

                if (c == '\n')
                {
                    cursor.Next();
                    sb.Append('\n');
                    SkipAlignment(alignColumn);
                    continue;
                }

                sb.Append(cursor.Next());
            }

            cursor.SkipWhitespace();
            terminator = cursor.Peek();
            if (terminator == ';')
            {
                cursor.Next();
            }
            else if (terminator != '}')
            {
                throw cursor.Error($"';' or '}}' expected after bracketed value but found '{cursor.Describe()}'.");
            }

            return sb.ToString();
        }

        var valueColumn = cursor.Column - 1;
        while (true)
        {
            var c = cursor.Peek();
            if (c == '\0')
            {
                throw cursor.Error("Unterminated property value; ';' or '}' expected.");
            }

            if (c is ';' or '}')
            {
                terminator = c;
                if (c == ';')
                {
                    cursor.Next();
                }

                break;
            }

            if (c == '\r')
            {
                cursor.Next();
                continue;
            }

            if (c == '\n')
            {
                cursor.Next();
                TrimTrailingSpaces(sb);
                sb.Append('\n');
                SkipAlignment(valueColumn);
                continue;
            }

            sb.Append(cursor.Next());
        }

        TrimTrailingSpaces(sb);
        var result = sb.ToString();

        // A value whose continuation lines were all blank ends with '\n'; drop trailing line breaks.
        return result.TrimEnd('\n', ' ');
    }

    private void SkipAlignment(int column)
    {
        var skipped = 0;
        while (skipped < column && cursor.Peek() is ' ' or '\t')
        {
            cursor.Next();
            skipped++;
        }
    }

    private static void TrimTrailingSpaces(StringBuilder sb)
    {
        while (sb.Length > 0 && sb[^1] is ' ' or '\t')
        {
            sb.Length--;
        }
    }

    /// <summary>Parses <c>[VAR declarations] BEGIN … END;</c> of a trigger written as a property.</summary>
    private CalTrigger ParseTrigger(string name, SourcePosition position)
    {
        cursor.SkipSpaces();
        var variables = new List<CalVariable>();
        if (cursor.TryReadWord("VAR"))
        {
            ParseDeclarations(variables, () => cursor.LookingAtWord("BEGIN"));
        }

        cursor.SkipWhitespace();
        var body = ParseBody();
        return new CalTrigger { Name = name, Variables = variables, Body = body, Position = position };
    }

    /// <summary>
    /// Parses <c>BEGIN</c>, the code lines, and the <c>END;</c> that is aligned with the <c>BEGIN</c>.
    /// Code lines lose the block's base indentation (BEGIN column + 2) but keep any further indentation.
    /// </summary>
    private CalCodeBody ParseBody()
    {
        var beginIndent = cursor.Column - 1;
        cursor.ExpectWord("BEGIN");
        cursor.SkipSpaces();
        if (!cursor.AtLineEnd && cursor.Peek() != '\r')
        {
            throw cursor.Error("Line break expected after BEGIN.");
        }

        cursor.ReadLine();
        var lines = new List<CodeLine>();
        var strip = beginIndent + 2;
        while (true)
        {
            if (cursor.AtEnd)
            {
                throw cursor.Error("Unterminated code block; 'END;' aligned with 'BEGIN' expected.");
            }

            var indent = cursor.CurrentLineIndent();
            var lineText = cursor.CurrentLineText();
            var content = lineText.TrimStart(' ', '\t');
            if (indent == beginIndent && content.StartsWith("END", StringComparison.OrdinalIgnoreCase)
                && content[3..].TrimStart().StartsWith(';'))
            {
                cursor.Advance(indent);
                cursor.Advance(3);
                cursor.SkipSpaces();
                cursor.Expect(';');
                break;
            }

            var sourceLine = cursor.Line;
            var raw = cursor.ReadLine();
            lines.Add(new CodeLine(StripIndent(raw, strip), sourceLine));
        }

        // Drop trailing empty lines (C/SIDE never emits them inside a body, but hand-edited files may).
        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1].Text))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return new CalCodeBody(lines);
    }

    private static string StripIndent(string line, int count)
    {
        var i = 0;
        while (i < count && i < line.Length && line[i] is ' ' or '\t')
        {
            i++;
        }

        return line[i..].TrimEnd();
    }

    // ------------------------------------------------------------------------------------------------
    // CODE section
    // ------------------------------------------------------------------------------------------------

    private void ParseCodeSection(CalObject obj)
    {
        OpenBlock();
        cursor.SkipWhitespace();
        if (cursor.TryReadWord("VAR"))
        {
            ParseDeclarations(obj.Code.Variables, () =>
                cursor.Peek() is '[' or '}' || cursor.LookingAtWord("PROCEDURE") || cursor.LookingAtWord("LOCAL")
                || cursor.LookingAtWord("EVENT") || cursor.LookingAtWord("BEGIN"));
        }

        while (true)
        {
            cursor.SkipWhitespace();
            if (cursor.Peek() == '}')
            {
                cursor.Next();
                return;
            }

            if (cursor.LookingAtWord("BEGIN"))
            {
                ParseDocumentation(obj.Code.Documentation);
                cursor.SkipWhitespace();
                cursor.Expect('}');
                return;
            }

            if (cursor.Peek() == '[' || cursor.LookingAtWord("PROCEDURE") || cursor.LookingAtWord("LOCAL") || cursor.LookingAtWord("EVENT"))
            {
                obj.Code.Procedures.Add(ParseProcedure());
                continue;
            }

            throw cursor.Error($"PROCEDURE, EVENT, VAR or BEGIN expected in CODE section but found '{cursor.Describe()}'.");
        }
    }

    private void ParseDocumentation(List<string> target)
    {
        cursor.ExpectWord("BEGIN");
        cursor.SkipWhitespace();
        if (cursor.Peek() == '{')
        {
            var braceIndent = cursor.Column - 1;
            cursor.Next();
            cursor.ReadLine();
            var lines = new List<string>();
            while (true)
            {
                if (cursor.AtEnd)
                {
                    throw cursor.Error("Unterminated documentation block.");
                }

                var line = cursor.CurrentLineText();
                if (line.Trim() == "}" && cursor.CurrentLineIndent() <= braceIndent)
                {
                    cursor.ReadLine();
                    break;
                }

                lines.Add(cursor.ReadLine());
            }

            // Remove the common indentation C/SIDE adds (normally 6 spaces).
            var common = lines.Where(l => l.Trim().Length > 0).Select(l => l.Length - l.TrimStart(' ').Length).DefaultIfEmpty(0).Min();
            target.AddRange(lines.Select(l => (l.Length >= common ? l[common..] : l.TrimStart()).TrimEnd()));
            while (target.Count > 0 && target[^1].Length == 0)
            {
                target.RemoveAt(target.Count - 1);
            }
        }

        cursor.SkipWhitespace();
        cursor.ExpectWord("END");
        cursor.SkipSpaces();
        cursor.Expect('.');
    }

    private CalProcedure ParseProcedure()
    {
        var position = cursor.SourcePosition;
        var attributes = new List<string>();
        while (cursor.Peek() == '[')
        {
            attributes.Add(ReadAttribute());
            cursor.SkipWhitespace();
        }

        position = attributes.Count > 0 ? position : cursor.SourcePosition;
        var isLocal = cursor.TryReadWord("LOCAL");
        cursor.SkipWhitespace();

        string name;
        int? id;
        string? eventSource = null;
        int? eventSourceId = null;
        var isEvent = false;

        if (cursor.TryReadWord("PROCEDURE"))
        {
            cursor.SkipSpaces();
            name = ReadName();
            id = ReadOptionalId();
        }
        else if (cursor.TryReadWord("EVENT"))
        {
            isEvent = true;
            cursor.SkipSpaces();
            eventSource = ReadName();
            eventSourceId = ReadOptionalId();
            cursor.SkipSpaces();
            cursor.Expect(':');
            cursor.Expect(':');
            name = ReadName();
            id = ReadOptionalId();
        }
        else
        {
            throw cursor.Error($"PROCEDURE or EVENT expected but found '{cursor.Describe()}'.");
        }

        var parameters = new List<CalVariable>();
        cursor.SkipSpaces();
        if (cursor.Peek() == '(')
        {
            cursor.Next();
            cursor.SkipWhitespace();
            if (cursor.Peek() != ')')
            {
                while (true)
                {
                    parameters.Add(ParseVariable(isParameter: true));
                    cursor.SkipWhitespace();
                    if (cursor.Peek() == ';')
                    {
                        cursor.Next();
                        cursor.SkipWhitespace();
                        continue;
                    }

                    break;
                }
            }

            cursor.Expect(')');
        }

        cursor.SkipSpaces();
        CalVariable? returnValue = null;
        if (cursor.Peek() != ';')
        {
            returnValue = ParseVariable(isParameter: false);
            cursor.SkipSpaces();
        }

        cursor.Expect(';');
        cursor.SkipWhitespace();

        var variables = new List<CalVariable>();
        if (cursor.TryReadWord("VAR"))
        {
            ParseDeclarations(variables, () => cursor.LookingAtWord("BEGIN"));
            cursor.SkipWhitespace();
        }

        var body = ParseBody();
        return new CalProcedure
        {
            Name = name,
            Id = id,
            IsLocal = isLocal,
            Attributes = attributes,
            Parameters = parameters,
            ReturnValue = returnValue,
            Variables = variables,
            Body = body,
            IsEvent = isEvent,
            EventSource = eventSource,
            EventSourceId = eventSourceId,
            Position = position,
        };
    }

    /// <summary>Reads <c>[ … ]</c> respecting quotes and parentheses; returns the content without the brackets.</summary>
    private string ReadAttribute()
    {
        cursor.Expect('[');
        var sb = new StringBuilder();
        var depth = 0;
        char quote = '\0';
        while (true)
        {
            var c = cursor.Peek();
            if (c is '\0' or '\n')
            {
                throw cursor.Error("Unterminated attribute; ']' expected.");
            }

            cursor.Next();
            if (quote != '\0')
            {
                if (c == quote)
                {
                    quote = '\0';
                }

                sb.Append(c);
                continue;
            }

            switch (c)
            {
                case '"' or '\'':
                    quote = c;
                    break;
                case '(':
                    depth++;
                    break;
                case ')':
                    depth--;
                    break;
                case ']' when depth <= 0:
                    return sb.ToString().Trim();
            }

            sb.Append(c);
        }
    }

    // ------------------------------------------------------------------------------------------------
    // Declarations
    // ------------------------------------------------------------------------------------------------

    private void ParseDeclarations(List<CalVariable> target, Func<bool> atEnd)
    {
        while (true)
        {
            cursor.SkipWhitespace();
            if (cursor.AtEnd)
            {
                throw cursor.Error("Unexpected end of object in variable declarations.");
            }

            if (atEnd())
            {
                return;
            }

            if (cursor.Peek() == ';')
            {
                cursor.Next();
                continue;
            }

            target.Add(ParseVariable(isParameter: false));
            cursor.SkipWhitespace();
            cursor.Expect(';');
        }
    }

    /// <summary>
    /// Parses <c>[VAR] Name@Id : [ARRAY [d] OF] [TEMPORARY] Type [subtype] [modifiers]</c>. The terminator is not consumed.
    /// The name may be empty (unnamed return value).
    /// </summary>
    private CalVariable ParseVariable(bool isParameter)
    {
        cursor.SkipWhitespace();
        var position = cursor.SourcePosition;
        var isVar = false;
        if (isParameter && cursor.LookingAtWord("VAR"))
        {
            // "VAR" followed by a name (not "VAR@1000 :" which would be a variable named VAR).
            var mark = cursor.Save();
            cursor.Advance(3);
            cursor.SkipSpaces();
            if (cursor.Peek() is '@' or ':')
            {
                cursor.Restore(mark);
            }
            else
            {
                isVar = true;
            }
        }

        var name = cursor.Peek() == ':' ? string.Empty : ReadName();
        var id = ReadOptionalId();
        cursor.SkipWhitespace();
        cursor.Expect(':');
        cursor.SkipWhitespace();

        string? dimensions = null;
        if (cursor.TryReadWord("ARRAY"))
        {
            cursor.SkipSpaces();
            cursor.Expect('[');
            var sb = new StringBuilder();
            while (cursor.Peek() is not (']' or '\0' or '\n'))
            {
                sb.Append(cursor.Next());
            }

            cursor.Expect(']');
            dimensions = sb.ToString().Trim();
            cursor.SkipWhitespace();
            cursor.ExpectWord("OF");
            cursor.SkipWhitespace();
        }

        var temporary = false;
        if (cursor.TryReadWord("TEMPORARY"))
        {
            temporary = true;
            cursor.SkipWhitespace();
        }

        var type = ParseDataType(out var runOnClient, out var withEvents, out var suppressDispose, out var nonDebuggableType);

        var inDataSet = false;
        string? securityFiltering = null;
        var nonDebuggable = nonDebuggableType;
        while (true)
        {
            cursor.SkipSpaces();
            if (cursor.TryReadWord("INDATASET"))
            {
                inDataSet = true;
                continue;
            }

            if (cursor.TryReadWord("SECURITYFILTERING"))
            {
                cursor.SkipSpaces();
                cursor.Expect('(');
                var sb = new StringBuilder();
                while (cursor.Peek() is not (')' or '\0' or '\n'))
                {
                    sb.Append(cursor.Next());
                }

                cursor.Expect(')');
                securityFiltering = sb.ToString().Trim();
                continue;
            }

            if (cursor.TryReadWord("NONDEBUGGABLE"))
            {
                nonDebuggable = true;
                continue;
            }

            break;
        }

        return new CalVariable
        {
            Name = name,
            Id = id,
            Type = type,
            IsVar = isVar,
            IsTemporary = temporary,
            Dimensions = dimensions,
            InDataSet = inDataSet,
            RunOnClient = runOnClient,
            WithEvents = withEvents,
            SuppressDispose = suppressDispose,
            NonDebuggable = nonDebuggable,
            SecurityFiltering = securityFiltering,
            Position = position,
        };
    }

    [GeneratedRegex(@"^(?<name>Code|Text)(?<len>\d+)$", RegexOptions.IgnoreCase)]
    private static partial Regex TypeWithLengthRegex();

    private CalDataType ParseDataType(out bool runOnClient, out bool withEvents, out bool suppressDispose, out bool nonDebuggable)
    {
        runOnClient = withEvents = suppressDispose = nonDebuggable = false;

        if (cursor.Peek() == '\'')
        {
            return new CalDataType { Name = "Option", OptionString = ReadQuotedString('\'') };
        }

        var word = cursor.ReadWord();
        if (word.Length == 0)
        {
            throw cursor.Error($"Data type expected but found '{cursor.Describe()}'.");
        }

        switch (word.ToUpperInvariant())
        {
            case "RECORD":
            case "CODEUNIT":
            case "PAGE":
            case "REPORT":
            case "XMLPORT":
            case "QUERY":
            case "TESTPAGE":
            case "TESTREQUESTPAGE":
            case "FORM":
            case "DATAPORT":
            {
                cursor.SkipSpaces();
                var subtype = cursor.Peek() == '"' ? "\"" + ReadQuotedString('"') + "\"" : cursor.ReadWord();
                if (subtype.Length == 0)
                {
                    throw cursor.Error($"Object number or name expected after {word}.");
                }

                return new CalDataType { Name = word, Subtype = subtype };
            }

            case "DOTNET":
            case "AUTOMATION":
            case "OCX":
            {
                cursor.SkipSpaces();
                string? external = null;
                if (cursor.Peek() == '"')
                {
                    external = ReadQuotedString('"');
                }

                while (true)
                {
                    cursor.SkipSpaces();
                    if (cursor.TryReadWord("RUNONCLIENT"))
                    {
                        runOnClient = true;
                    }
                    else if (cursor.TryReadWord("WITHEVENTS"))
                    {
                        withEvents = true;
                    }
                    else if (cursor.TryReadWord("SUPPRESSDISPOSE"))
                    {
                        suppressDispose = true;
                    }
                    else if (cursor.TryReadWord("NONDEBUGGABLE"))
                    {
                        nonDebuggable = true;
                    }
                    else
                    {
                        break;
                    }
                }

                return new CalDataType { Name = word, ExternalTypeName = external };
            }

            case "TEXTCONST":
            {
                cursor.SkipSpaces();
                if (cursor.Peek() != '\'')
                {
                    return new CalDataType { Name = word, TextConstValue = string.Empty };
                }

                var value = new StringBuilder(ReadQuotedString('\''));
                while (true)
                {
                    var mark = cursor.Save();
                    cursor.SkipWhitespace();
                    if (cursor.Peek() == ',')
                    {
                        cursor.Next();
                        cursor.SkipWhitespace();
                        value.Append(';').Append(ReadQuotedString('\''));
                        continue;
                    }

                    cursor.Restore(mark);
                    break;
                }

                return new CalDataType { Name = word, TextConstValue = value.ToString() };
            }

            case "OPTION":
            {
                cursor.SkipSpaces();
                string? options = null;
                if (cursor.Peek() == '\'')
                {
                    options = ReadQuotedString('\'');
                }

                return new CalDataType { Name = "Option", OptionString = options };
            }
        }

        var match = TypeWithLengthRegex().Match(word);
        if (match.Success)
        {
            return new CalDataType
            {
                Name = match.Groups["name"].Value,
                Length = int.Parse(match.Groups["len"].Value, CultureInfo.InvariantCulture),
            };
        }

        cursor.SkipSpaces();
        if (cursor.Peek() == '[')
        {
            cursor.Next();
            var digits = cursor.ReadWord();
            cursor.Expect(']');
            if (int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var length))
            {
                return new CalDataType { Name = word, Length = length };
            }

            throw cursor.Error($"Invalid length '{digits}' for type {word}.");
        }

        return new CalDataType { Name = word };
    }

    /// <summary>Reads a quoted identifier or a plain word.</summary>
    private string ReadName()
    {
        if (cursor.Peek() == '"')
        {
            return ReadQuotedString('"');
        }

        var word = cursor.ReadWord();
        if (word.Length == 0)
        {
            throw cursor.Error($"Name expected but found '{cursor.Describe()}'.");
        }

        return word;
    }

    private int? ReadOptionalId()
    {
        cursor.SkipSpaces();
        if (cursor.Peek() != '@')
        {
            return null;
        }

        cursor.Next();
        var negative = false;
        if (cursor.Peek() == '-')
        {
            negative = true;
            cursor.Next();
        }

        var digits = cursor.ReadWord();
        if (!int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
        {
            throw cursor.Error($"Number expected after '@' but found '{digits}'.");
        }

        return negative ? -id : id;
    }

    /// <summary>Reads a string in <paramref name="quote"/> characters; a doubled quote stands for one quote character.</summary>
    private string ReadQuotedString(char quote)
    {
        cursor.Expect(quote);
        var sb = new StringBuilder();
        while (true)
        {
            var c = cursor.Peek();
            if (c is '\0' or '\n')
            {
                throw cursor.Error($"Unterminated string; {quote} expected.");
            }

            cursor.Next();
            if (c == quote)
            {
                if (cursor.Peek() == quote)
                {
                    cursor.Next();
                    sb.Append(quote);
                    continue;
                }

                return sb.ToString();
            }

            sb.Append(c);
        }
    }

    // ------------------------------------------------------------------------------------------------
    // RDLDATA / WORDLAYOUT
    // ------------------------------------------------------------------------------------------------

    private void ParseRawBlock(List<string> target, string endMarker)
    {
        OpenBlock();
        cursor.ReadLine();
        while (true)
        {
            if (cursor.AtEnd)
            {
                throw cursor.Error($"Unterminated block; '{endMarker}' expected.");
            }

            var line = cursor.ReadLine();
            var trimmed = line.Trim();
            if (target.Count == 0 && trimmed == "}")
            {
                return; // empty block
            }

            if (trimmed.Equals(endMarker, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            target.Add(line);
        }

        cursor.SkipWhitespace();
        cursor.Expect('}');
    }
}
