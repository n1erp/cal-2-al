using System.Globalization;
using System.Text;
using Cal2AL.Parser.Code;

namespace Cal2AL.Converter.Comparison;

public enum OutlineEntryKind
{
    Object,
    Element,
    Property,
    Variable,
    Method,
}

/// <summary>
/// One comparable part of an AL object: a property, a variable, a method (signature, locals and body) or an element
/// header (<c>field(1; "No."; Code[20])</c>). <see cref="Path"/> identifies it independently of its position.
/// </summary>
public sealed record OutlineEntry(string Path, OutlineEntryKind Kind, IReadOnlyList<CodeToken> Tokens, int Line)
{
    /// <summary>The tokens as written (single spaces between tokens).</summary>
    public string Text => string.Join(" ", Tokens.Select(t => t.Text));
}

/// <summary>An AL object reduced to keyed entries, for comparing two conversions of the same C/AL object.</summary>
public sealed class AlOutline
{
    public required string Kind { get; init; }
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string File { get; init; }
    public List<OutlineEntry> Entries { get; } = [];

    /// <summary>Key used to pair objects across two folders: kind and ID (or name for ID-less objects).</summary>
    public string Key => $"{Kind.ToLowerInvariant()} {(Id.Length > 0 ? Id : Name.ToLowerInvariant())}";
}

/// <summary>
/// A tolerant structural reader for AL files. It does not validate AL; it only finds blocks, properties, variables
/// and methods so that two files can be compared entry by entry.
/// </summary>
public static class AlOutlineReader
{
    private static readonly HashSet<string> MethodModifiers = new(StringComparer.OrdinalIgnoreCase) { "local", "internal", "protected" };

    public static List<AlOutline> Read(string text, string file)
    {
        var tokens = CodeLexer.Tokenize(text, CodeLanguage.AL).Where(t => !t.IsTrivia && t.Kind != CodeTokenKind.EndOfInput).ToList();
        var reader = new Cursor(tokens);
        var result = new List<AlOutline>();
        while (!reader.AtEnd)
        {
            var start = reader.Position;
            if (reader.Current.Kind is not (CodeTokenKind.Identifier) || reader.Current.Text.StartsWith('#'))
            {
                reader.Next();
                continue;
            }

            var kind = reader.Next().Text;
            var id = reader.Current.Kind == CodeTokenKind.Integer ? reader.Next().Text : string.Empty;
            var header = new List<CodeToken>();
            while (!reader.AtEnd && !reader.IsOpenBrace)
            {
                header.Add(reader.Next());
            }

            if (reader.AtEnd)
            {
                break;
            }

            var name = header.Count > 0 ? Unquote(header[0]) : string.Empty;
            var outline = new AlOutline { Kind = kind, Id = id, Name = name, File = file };
            outline.Entries.Add(new OutlineEntry("(object)", OutlineEntryKind.Object, tokens.GetRange(start, reader.Position - start), tokens[start].Line));
            reader.Next(); // {
            ReadBlock(reader, outline, string.Empty);
            result.Add(outline);
        }

        return result;
    }

    private static void ReadBlock(Cursor reader, AlOutline outline, string path)
    {
        var keys = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var attributes = new List<CodeToken>();
        var inVar = false;

        string Unique(string key)
        {
            var n = keys.TryGetValue(key, out var count) ? count + 1 : 1;
            keys[key] = n;
            return n == 1 ? key : $"{key}#{n.ToString(CultureInfo.InvariantCulture)}";
        }

        void Add(string key, OutlineEntryKind kind, List<CodeToken> tokens) =>
            outline.Entries.Add(new OutlineEntry(Unique(path.Length == 0 ? key : $"{path}/{key}"), kind, tokens, tokens.Count > 0 ? tokens[0].Line : 0));

        while (!reader.AtEnd)
        {
            var token = reader.Current;
            if (reader.IsCloseBrace)
            {
                reader.Next();
                return;
            }

            if (token.Kind == CodeTokenKind.OpenBracket)
            {
                attributes.AddRange(reader.ReadBalanced(CodeTokenKind.OpenBracket, CodeTokenKind.CloseBracket));
                continue;
            }

            if (token.Is("var"))
            {
                inVar = true;
                reader.Next();
                continue;
            }

            var isMethod = token.Is("trigger") || token.Is("procedure")
                || (MethodModifiers.Contains(token.Text) && reader.Peek(1) is { } next && (next.Is("procedure") || MethodModifiers.Contains(next.Text)));
            if (isMethod)
            {
                inVar = false;
                var method = new List<CodeToken>(attributes);
                attributes.Clear();
                while (!reader.AtEnd && !reader.Current.Is("procedure") && !reader.Current.Is("trigger"))
                {
                    method.Add(reader.Next());
                }

                method.Add(reader.Next());
                var nameTokens = new List<CodeToken>();
                while (!reader.AtEnd && reader.Current.Kind != CodeTokenKind.OpenParen)
                {
                    nameTokens.Add(reader.Next());
                }

                method.AddRange(nameTokens);
                method.AddRange(reader.ReadMethodRest());
                var methodName = string.Concat(nameTokens.Select(Unquote));
                Add($"{(method.Any(t => t.Is("trigger")) ? "trigger" : "procedure")} {methodName}", OutlineEntryKind.Method, method);
                continue;
            }

            if (inVar && token.IsIdentifier && reader.Peek(1) is { Kind: CodeTokenKind.Colon or CodeTokenKind.Comma })
            {
                var declaration = new List<CodeToken>(attributes);
                attributes.Clear();
                declaration.AddRange(reader.ReadUntilSemicolon());
                var names = declaration.TakeWhile(t => t.Kind != CodeTokenKind.Colon).Where(t => t.IsIdentifier).Select(Unquote);
                Add($"var {string.Join(",", names)}", OutlineEntryKind.Variable, declaration);
                continue;
            }

            if (token.IsIdentifier && reader.Peek(1) is { Kind: CodeTokenKind.Equal })
            {
                inVar = false;
                var property = reader.ReadUntilSemicolon();
                Add($"{Unquote(token)}", OutlineEntryKind.Property, property);
                continue;
            }

            if (token.IsIdentifier && reader.Peek(1) is { Kind: CodeTokenKind.OpenParen })
            {
                inVar = false;
                var header = new List<CodeToken> { reader.Next() };
                header.AddRange(reader.ReadBalanced(CodeTokenKind.OpenParen, CodeTokenKind.CloseParen));
                var firstArgument = header.Skip(2).TakeWhile(t => t.Kind is not (CodeTokenKind.Semicolon or CodeTokenKind.CloseParen)).Select(Unquote);
                var key = $"{token.Text.ToLowerInvariant()}({string.Concat(firstArgument).ToLowerInvariant()})";
                if (reader.IsOpenBrace)
                {
                    reader.Next();
                    var childPath = Unique(path.Length == 0 ? key : $"{path}/{key}");
                    outline.Entries.Add(new OutlineEntry(childPath, OutlineEntryKind.Element, header, token.Line));
                    ReadBlock(reader, outline, childPath);
                }
                else
                {
                    Add(key, OutlineEntryKind.Element, header);
                }

                continue;
            }

            if (token.IsIdentifier && reader.Peek(1) is { } brace && IsOpenBrace(brace))
            {
                inVar = false;
                reader.Next();
                reader.Next();
                var childPath = Unique(path.Length == 0 ? token.Text.ToLowerInvariant() : $"{path}/{token.Text.ToLowerInvariant()}");
                ReadBlock(reader, outline, childPath);
                continue;
            }

            reader.Next();
        }
    }

    private static bool IsOpenBrace(CodeToken token) => token.Kind == CodeTokenKind.Unknown && token.Text == "{";

    private static string Unquote(CodeToken token) =>
        token.Kind == CodeTokenKind.QuotedIdentifier ? token.Text.Trim('"') : token.Text;

    private sealed class Cursor(List<CodeToken> tokens)
    {
        public int Position { get; private set; }
        public bool AtEnd => Position >= tokens.Count;
        public CodeToken Current => tokens[Position];
        public bool IsOpenBrace => !AtEnd && AlOutlineReader.IsOpenBrace(Current);
        public bool IsCloseBrace => !AtEnd && Current.Kind == CodeTokenKind.Unknown && Current.Text == "}";

        public CodeToken Next() => tokens[Position++];

        public CodeToken? Peek(int offset) => Position + offset < tokens.Count ? tokens[Position + offset] : null;

        public List<CodeToken> ReadBalanced(CodeTokenKind open, CodeTokenKind close)
        {
            var result = new List<CodeToken>();
            var depth = 0;
            while (!AtEnd)
            {
                var t = Next();
                result.Add(t);
                if (t.Kind == open)
                {
                    depth++;
                }
                else if (t.Kind == close && --depth == 0)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>Reads up to and including the next ';' outside parentheses and brackets.</summary>
        public List<CodeToken> ReadUntilSemicolon()
        {
            var result = new List<CodeToken>();
            var depth = 0;
            while (!AtEnd && !IsCloseBrace)
            {
                var t = Next();
                result.Add(t);
                if (t.Kind is CodeTokenKind.OpenParen or CodeTokenKind.OpenBracket)
                {
                    depth++;
                }
                else if (t.Kind is CodeTokenKind.CloseParen or CodeTokenKind.CloseBracket)
                {
                    depth--;
                }
                else if (t.Kind == CodeTokenKind.Semicolon && depth <= 0)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>Reads a method's parameters, return type, locals and body up to the matching <c>end</c>.</summary>
        public List<CodeToken> ReadMethodRest()
        {
            var result = new List<CodeToken>();
            while (!AtEnd && !Current.Is("begin"))
            {
                if (IsCloseBrace)
                {
                    return result;
                }

                result.Add(Next());
            }

            var depth = 0;
            while (!AtEnd)
            {
                var t = Next();
                result.Add(t);
                if (t.Is("begin") || t.Is("case"))
                {
                    depth++;
                }
                else if (t.Is("end") && --depth == 0)
                {
                    break;
                }
            }

            if (!AtEnd && Current.Kind == CodeTokenKind.Semicolon)
            {
                result.Add(Next());
            }

            return result;
        }
    }

    /// <summary>Debug helper: the outline as text, one entry per line.</summary>
    public static string Dump(AlOutline outline)
    {
        var sb = new StringBuilder();
        foreach (var entry in outline.Entries)
        {
            sb.Append(entry.Kind).Append(' ').Append(entry.Path).Append(" = ").AppendLine(entry.Text);
        }

        return sb.ToString();
    }
}
