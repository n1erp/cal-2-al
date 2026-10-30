using Cal2AL.Diagnostics;
using Cal2AL.Generator;

namespace Cal2AL.Converter.Values;

/// <summary>
/// Turns C/AL multi-language values (CaptionML, ToolTipML, TextConst, …) into AL label syntax
/// (<c>'Text', Comment = '…', Locked = true</c>) and records every language value for the translation files,
/// so no translation is lost.
/// </summary>
public static class LabelConverter
{
    /// <summary>Converts the value; returns null when the value is empty (an empty caption has no effect).</summary>
    /// <param name="context">Object being converted.</param>
    /// <param name="value">Multi-language value as written in the TXT file.</param>
    /// <param name="path">Readable location for the translation file, e.g. <c>Field No. - Property Caption</c>.</param>
    /// <param name="line">Source line, for diagnostics.</param>
    public static string? Convert(ObjectContext context, string value, string path, int? line)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var entries = MultiLanguageText.Parse(value.Trim());
        if (entries is null || entries.Count == 0)
        {
            context.Report(DiagnosticDescriptors.PropertyValueNotParsed,
                $"'{Shorten(value)}' is not a valid multi-language value; it was used as plain text.",
                line, construct: path, original: value);
            return AlSyntax.String(value.Replace("\n", " ", StringComparison.Ordinal));
        }

        var comment = entries.FirstOrDefault(e => e.IsComment)?.Text;
        var languages = entries.Where(e => !e.IsComment).ToList();
        var source = languages.FirstOrDefault(e => e.LanguageCode == "ENU") ?? languages.FirstOrDefault();
        if (source is null)
        {
            // Only a comment: the text is empty.
            return null;
        }

        if (source.LanguageCode != "ENU" && languages.Count > 0)
        {
            context.Report(DiagnosticDescriptors.PropertyValueAdjusted,
                $"No ENU text in '{Shorten(value)}'; the {source.LanguageCode} text is used as the AL source text.",
                line, construct: path);
        }

        var label = AlSyntax.String(source.Text);
        var locked = false;
        string? commentText = null;
        if (comment is not null)
        {
            var c = comment.Trim();
            if (c.StartsWith("{Locked}", StringComparison.OrdinalIgnoreCase))
            {
                locked = true;
                c = c["{Locked}".Length..].Trim();
            }

            if (c.Length > 0)
            {
                commentText = c;
                label += ", Comment = " + AlSyntax.String(c);
            }
        }

        if (locked)
        {
            label += ", Locked = true";
        }

        var translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in languages.Where(e => !ReferenceEquals(e, source)))
        {
            var culture = MultiLanguageText.ToCultureName(entry.LanguageCode);
            if (culture is null)
            {
                context.Report(DiagnosticDescriptors.PropertyValueAdjusted,
                    $"Unknown language code '{entry.LanguageCode}'; its text was not written to a translation file.",
                    line, construct: path, original: entry.Text, severity: DiagnosticSeverity.Warning);
                continue;
            }

            translations[culture] = entry.Text;
        }

        if (translations.Count > 0 && context.Options.GenerateTranslations && !locked)
        {
            context.Run.Translations.Add(new TranslationUnit(
                context.Object.Type.ToString(), context.Object.Name, path, source.Text, commentText, translations));
        }

        return label;
    }

    private static string Shorten(string value)
    {
        var flat = value.Replace("\n", " ", StringComparison.Ordinal);
        return flat.Length > 60 ? flat[..60] + "…" : flat;
    }
}
