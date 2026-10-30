using System.Globalization;
using System.Text.Json.Serialization;
using Cal2AL.Diagnostics;
using Cal2AL.Parser.Code;

namespace Cal2AL.Converter.Comparison;

/// <summary>How an entry of the Cal2AL output relates to the same entry of the reference (Txt2Al) output.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<DifferenceCategory>))]
public enum DifferenceCategory
{
    /// <summary>Same tokens.</summary>
    Equivalent,

    /// <summary>Differs only in case, quoting of plain identifiers, comments, whitespace or position.</summary>
    Formatting,

    /// <summary>Different tokens: the two outputs may behave differently.</summary>
    Behavioral,

    /// <summary>Present in the reference output only.</summary>
    MissingInCal2AL,

    /// <summary>Present in the Cal2AL output only (typically something the reference dropped or commented out).</summary>
    MissingInReference,
}

public sealed record EntryDifference(string Path, string Kind, DifferenceCategory Category, string? Cal2AL, string? Reference);

public sealed record ObjectComparison
{
    public required string Key { get; init; }
    public string? Cal2ALFile { get; init; }
    public string? ReferenceFile { get; init; }
    public List<EntryDifference> Entries { get; init; } = [];

    public int Count(DifferenceCategory category) => Entries.Count(e => e.Category == category);
}

public sealed record ComparisonReport
{
    public required string Cal2ALFolder { get; init; }
    public required string ReferenceFolder { get; init; }
    public List<ObjectComparison> Objects { get; init; } = [];

    public Dictionary<DifferenceCategory, int> Totals => Enum.GetValues<DifferenceCategory>()
        .ToDictionary(c => c, c => Objects.Sum(o => o.Count(c)));

    public string ToJson() => System.Text.Json.JsonSerializer.Serialize(this, ConversionReport.JsonOptions);
}

/// <summary>
/// Compares two folders of AL files (Cal2AL output and a reference such as Txt2Al output) object by object and
/// entry by entry, and classifies each difference.
/// </summary>
public static class AlComparer
{
    public static ComparisonReport CompareFolders(string cal2alFolder, string referenceFolder)
    {
        var mine = ReadFolder(cal2alFolder);
        var reference = ReadFolder(referenceFolder);
        var report = new ComparisonReport { Cal2ALFolder = cal2alFolder, ReferenceFolder = referenceFolder };
        foreach (var key in mine.Keys.Union(reference.Keys, StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase))
        {
            mine.TryGetValue(key, out var a);
            reference.TryGetValue(key, out var b);
            report.Objects.Add(Compare(key, a, b));
        }

        return report;
    }

    public static ObjectComparison Compare(string key, AlOutline? cal2al, AlOutline? reference)
    {
        var result = new ObjectComparison { Key = key, Cal2ALFile = cal2al?.File, ReferenceFile = reference?.File };
        var a = (cal2al?.Entries ?? []).ToDictionary(e => e.Path, StringComparer.OrdinalIgnoreCase);
        var b = (reference?.Entries ?? []).ToDictionary(e => e.Path, StringComparer.OrdinalIgnoreCase);

        foreach (var path in a.Keys.Union(b.Keys, StringComparer.OrdinalIgnoreCase))
        {
            a.TryGetValue(path, out var mineEntry);
            b.TryGetValue(path, out var referenceEntry);
            var kind = (mineEntry ?? referenceEntry)!.Kind.ToString();
            var category = (mineEntry, referenceEntry) switch
            {
                (null, _) => DifferenceCategory.MissingInCal2AL,
                (_, null) => DifferenceCategory.MissingInReference,
                _ => Classify(mineEntry.Tokens, referenceEntry.Tokens),
            };
            result.Entries.Add(new EntryDifference(path, kind, category,
                category == DifferenceCategory.Equivalent ? null : mineEntry?.Text,
                category == DifferenceCategory.Equivalent ? null : referenceEntry?.Text));
        }

        result.Entries.Sort((x, y) => string.CompareOrdinal(x.Path, y.Path));
        return result;
    }

    public static DifferenceCategory Classify(IReadOnlyList<CodeToken> a, IReadOnlyList<CodeToken> b)
    {
        var exactA = a.Where(t => !t.IsComment).Select(t => t.Text).ToList();
        var exactB = b.Where(t => !t.IsComment).Select(t => t.Text).ToList();
        if (exactA.SequenceEqual(exactB, StringComparer.Ordinal))
        {
            return DifferenceCategory.Equivalent;
        }

        return Normalize(a).SequenceEqual(Normalize(b), StringComparer.Ordinal)
            ? DifferenceCategory.Formatting
            : DifferenceCategory.Behavioral;
    }

    /// <summary>
    /// Case-insensitive for identifiers and keywords, quotes removed where not needed, numbers by value, trailing
    /// semicolons before <c>end</c> ignored.
    /// </summary>
    private static List<string> Normalize(IReadOnlyList<CodeToken> tokens)
    {
        var result = new List<string>();
        foreach (var t in tokens.Where(t => !t.IsComment))
        {
            var text = t.Kind switch
            {
                CodeTokenKind.QuotedIdentifier => t.Text.Trim('"').ToUpperInvariant(),
                CodeTokenKind.Identifier => t.Text.ToUpperInvariant(),
                CodeTokenKind.Integer or CodeTokenKind.Decimal or CodeTokenKind.BigInteger =>
                    decimal.TryParse(t.Text.TrimEnd('L', 'l'), NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d.ToString(CultureInfo.InvariantCulture) : t.Text,
                _ => t.Text,
            };

            if (text == "END" && result.Count > 0 && result[^1] == ";")
            {
                result.RemoveAt(result.Count - 1);
            }

            result.Add(text);
        }

        return result;
    }

    private static Dictionary<string, AlOutline> ReadFolder(string folder)
    {
        var result = new Dictionary<string, AlOutline>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(folder))
        {
            throw new DirectoryNotFoundException($"Folder '{folder}' does not exist.");
        }

        foreach (var file in Directory.EnumerateFiles(folder, "*.al", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            foreach (var outline in AlOutlineReader.Read(File.ReadAllText(file), Path.GetRelativePath(folder, file)))
            {
                var key = outline.Key;
                if (result.ContainsKey(key))
                {
                    key = $"{key} ({outline.File})";
                }

                result[key] = outline;
            }
        }

        return result;
    }
}
