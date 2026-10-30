using System.Globalization;
using System.Text;

namespace Cal2AL.Converter.Values;

/// <summary>One language entry of a C/AL multi-language value.</summary>
public sealed record MultiLanguageEntry(string LanguageCode, string Text)
{
    /// <summary>True for the developer comment entry (<c>@@@=…</c>).</summary>
    public bool IsComment => LanguageCode == "@@@";
}

/// <summary>
/// Parses C/AL multi-language values such as <c>ENU=Customer;DEU=Debitor;@@@=Comment</c>. Values containing
/// <c>;</c> or <c>=</c> or leading/trailing blanks are wrapped in double quotes, with <c>""</c> for a quote.
/// </summary>
public static class MultiLanguageText
{
    /// <summary>Windows three-letter language codes used by C/SIDE, mapped to culture names.</summary>
    private static readonly Dictionary<string, string> Cultures = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ENU"] = "en-US", ["ENG"] = "en-GB", ["ENA"] = "en-AU", ["ENC"] = "en-CA", ["ENZ"] = "en-NZ", ["ENI"] = "en-IE",
        ["DEU"] = "de-DE", ["DEA"] = "de-AT", ["DES"] = "de-CH", ["DEL"] = "de-LU",
        ["FRA"] = "fr-FR", ["FRB"] = "fr-BE", ["FRC"] = "fr-CA", ["FRS"] = "fr-CH", ["FRL"] = "fr-LU",
        ["NLD"] = "nl-NL", ["NLB"] = "nl-BE", ["DAN"] = "da-DK", ["SVE"] = "sv-SE", ["NOR"] = "nb-NO", ["FIN"] = "fi-FI",
        ["ISL"] = "is-IS", ["ESP"] = "es-ES", ["ESN"] = "es-ES", ["ESM"] = "es-MX", ["ITA"] = "it-IT", ["ITS"] = "it-CH",
        ["PTB"] = "pt-BR", ["PTG"] = "pt-PT", ["CSY"] = "cs-CZ", ["PLK"] = "pl-PL", ["HUN"] = "hu-HU", ["RUS"] = "ru-RU",
        ["SKY"] = "sk-SK", ["SLV"] = "sl-SI", ["HRV"] = "hr-HR", ["SRL"] = "sr-Latn-RS", ["ROM"] = "ro-RO", ["BGR"] = "bg-BG",
        ["ELL"] = "el-GR", ["TRK"] = "tr-TR", ["ETI"] = "et-EE", ["LVI"] = "lv-LV", ["LTH"] = "lt-LT", ["UKR"] = "uk-UA",
        ["CHS"] = "zh-CN", ["CHT"] = "zh-TW", ["JPN"] = "ja-JP", ["KOR"] = "ko-KR", ["THA"] = "th-TH", ["HEB"] = "he-IL",
        ["ARA"] = "ar-SA", ["IND"] = "id-ID", ["MSL"] = "ms-MY", ["VIT"] = "vi-VN",
    };

    public static string? ToCultureName(string languageCode) =>
        Cultures.TryGetValue(languageCode, out var name) ? name : FindCulture(languageCode);

    private static string? FindCulture(string code)
    {
        try
        {
            return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .FirstOrDefault(c => string.Equals(c.ThreeLetterWindowsLanguageName, code, StringComparison.OrdinalIgnoreCase))?.Name;
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }

    /// <summary>True when the value looks like a multi-language value (starts with a language code and '=').</summary>
    public static bool LooksMultiLanguage(string value)
    {
        var v = value.TrimStart();
        return v.Length >= 4 && v[3] == '=' && (v.StartsWith("@@@", StringComparison.Ordinal) || v[..3].All(char.IsAsciiLetterUpper));
    }

    /// <summary>Parses the value. Returns null when it is not a well-formed multi-language value.</summary>
    public static IReadOnlyList<MultiLanguageEntry>? Parse(string value)
    {
        var entries = new List<MultiLanguageEntry>();
        var pos = 0;
        while (pos < value.Length)
        {
            while (pos < value.Length && char.IsWhiteSpace(value[pos]))
            {
                pos++;
            }

            if (pos >= value.Length)
            {
                break;
            }

            if (pos + 3 >= value.Length || value[pos + 3] != '=')
            {
                return null;
            }

            var code = value.Substring(pos, 3);
            if (code != "@@@" && !code.All(char.IsAsciiLetter))
            {
                return null;
            }

            pos += 4;
            string text;
            if (pos < value.Length && value[pos] == '"')
            {
                var sb = new StringBuilder();
                pos++;
                while (true)
                {
                    if (pos >= value.Length)
                    {
                        return null;
                    }

                    if (value[pos] == '"')
                    {
                        if (pos + 1 < value.Length && value[pos + 1] == '"')
                        {
                            sb.Append('"');
                            pos += 2;
                            continue;
                        }

                        pos++;
                        break;
                    }

                    sb.Append(value[pos++]);
                }

                text = sb.ToString();
                while (pos < value.Length && value[pos] != ';')
                {
                    if (!char.IsWhiteSpace(value[pos]))
                    {
                        return null;
                    }

                    pos++;
                }

                pos++;
            }
            else
            {
                var end = value.IndexOf(';', pos);
                text = end < 0 ? value[pos..] : value[pos..end];
                pos = end < 0 ? value.Length : end + 1;
                text = text.TrimEnd('\n', '\r');
            }

            entries.Add(new MultiLanguageEntry(code.ToUpperInvariant(), text));
        }

        return entries;
    }
}
