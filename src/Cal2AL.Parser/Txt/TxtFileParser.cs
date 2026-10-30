using System.Text;
using System.Text.RegularExpressions;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;

namespace Cal2AL.Parser.Txt;

/// <summary>An object whose text could not be parsed. Its raw text is kept so nothing is lost.</summary>
public sealed record FailedObject(
    CalObjectType Type,
    int Id,
    string Name,
    string SourceFile,
    int Line,
    string RawText,
    Diagnostic Diagnostic);

/// <summary>The result of reading one TXT file.</summary>
public sealed class TxtFileParseResult
{
    public required string SourceFile { get; init; }
    public List<CalObject> Objects { get; } = [];
    public List<FailedObject> Failures { get; } = [];
    public List<Diagnostic> Diagnostics { get; } = [];
}

/// <summary>
/// Reads a TXT/delta file that may contain any number of objects. The file is first split at lines that start with
/// <c>OBJECT </c> in column 1 (code and property values are always indented, so this is unambiguous), and each
/// chunk is parsed on its own. A malformed object therefore only affects itself — unlike Txt2Al, where one bad
/// object discards every object in the file.
/// </summary>
public sealed partial class TxtFileParser
{
    private readonly Encoding fallbackEncoding;

    public TxtFileParser(Encoding? fallbackEncoding = null)
    {
        this.fallbackEncoding = fallbackEncoding ?? TextDecoder.ResolveEncoding("850");
    }

    [GeneratedRegex(@"^OBJECT\s+\S")]
    private static partial Regex ObjectLineRegex();

    public TxtFileParseResult ParseFile(string path)
    {
        byte[] bytes;
        try
        {
            bytes = File.ReadAllBytes(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            var result = new TxtFileParseResult { SourceFile = path };
            result.Diagnostics.Add(DiagnosticDescriptors.FileReadError.Create(
                $"Cannot read '{path}': {ex.Message}", new DiagnosticLocation { File = path }, reason: ex.Message));
            return result;
        }

        var decoded = TextDecoder.Decode(bytes, fallbackEncoding);
        var parsed = ParseText(decoded.Text, path);
        if (decoded.UsedFallback)
        {
            parsed.Diagnostics.Add(DiagnosticDescriptors.EncodingFallback.Create(
                $"'{Path.GetFileName(path)}' is not UTF-8; decoded with code page {decoded.Encoding.CodePage} ({decoded.Encoding.WebName}).",
                new DiagnosticLocation { File = path }));
        }

        return parsed;
    }

    public TxtFileParseResult ParseText(string text, string sourceFile)
    {
        text = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').TrimEnd('\u001a');
        var result = new TxtFileParseResult { SourceFile = sourceFile };
        var lines = text.Split('\n');

        var starts = new List<int>();
        for (var i = 0; i < lines.Length; i++)
        {
            if (ObjectLineRegex().IsMatch(lines[i]))
            {
                starts.Add(i);
            }
        }

        if (starts.Count == 0)
        {
            if (text.Trim().Length > 0)
            {
                result.Diagnostics.Add(DiagnosticDescriptors.NotAnObjectFile.Create(
                    $"'{Path.GetFileName(sourceFile)}' contains no OBJECT declaration.", new DiagnosticLocation { File = sourceFile, Line = 1 }));
            }

            return result;
        }

        var leading = string.Join("\n", lines.Take(starts[0])).Trim();
        if (leading.Length > 0)
        {
            result.Diagnostics.Add(DiagnosticDescriptors.UnexpectedText.Create(
                "Text before the first OBJECT declaration was ignored.",
                new DiagnosticLocation { File = sourceFile, Line = 1 },
                originalSource: leading));
        }

        var parser = new TxtObjectParser();
        for (var s = 0; s < starts.Count; s++)
        {
            var start = starts[s];
            var end = s + 1 < starts.Count ? starts[s + 1] : lines.Length;
            while (end > start + 1 && lines[end - 1].Trim().Length == 0)
            {
                end--;
            }

            var chunk = string.Join("\n", lines, start, end - start);
            try
            {
                result.Objects.Add(parser.Parse(chunk, sourceFile, start));
            }
            catch (TxtParseException ex)
            {
                result.Failures.Add(CreateFailure(chunk, sourceFile, start, ex.Message, ex.Line, ex.Column));
            }
        }

        return result;
    }

    private static FailedObject CreateFailure(string chunk, string file, int lineOffset, string message, int line, int column)
    {
        var headerLine = chunk.Split('\n', 2)[0];
        var type = CalObjectType.Unknown;
        var id = 0;
        var name = headerLine.Length > 7 ? headerLine[7..].Trim() : headerLine;
        try
        {
            var header = TxtObjectParser.ParseHeader(headerLine.Length >= 6 ? headerLine[6..] : headerLine);
            (type, id, name) = (header.Type, header.Id, header.Name);
        }
        catch (TxtParseException)
        {
            // keep the defaults; the header itself is the problem and the message says so
        }

        var diagnostic = DiagnosticDescriptors.TxtSyntaxError.Create(
            message,
            new DiagnosticLocation
            {
                File = file,
                Line = line > lineOffset ? line : lineOffset + 1,
                Column = column,
                ObjectType = type == CalObjectType.Unknown ? null : type.ToString(),
                ObjectId = id,
                ObjectName = name,
            },
            construct: "TXT object",
            reason: message);
        return new FailedObject(type, id, name, file, lineOffset + 1, chunk, diagnostic);
    }
}
