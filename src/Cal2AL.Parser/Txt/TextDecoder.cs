using System.Text;

namespace Cal2AL.Parser.Txt;

/// <summary>Result of decoding a file.</summary>
public sealed record DecodedText(string Text, Encoding Encoding, bool UsedFallback);

/// <summary>
/// Decodes C/SIDE export files. C/SIDE writes TXT exports in the OEM code page of the machine (usually 850 in
/// Western Europe, 437 in the US); newer tools and editors may have re-saved them as UTF-8. Detection order:
/// byte order mark → strict UTF-8 → configured fallback code page.
/// </summary>
public static class TextDecoder
{
    static TextDecoder()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>Resolves an encoding name or code page number ("850", "ibm850", "windows-1252", "utf-8").</summary>
    public static Encoding ResolveEncoding(string nameOrCodePage)
    {
        return int.TryParse(nameOrCodePage, out var codePage)
            ? Encoding.GetEncoding(codePage)
            : Encoding.GetEncoding(nameOrCodePage);
    }

    public static DecodedText Decode(byte[] bytes, Encoding fallback)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        ArgumentNullException.ThrowIfNull(fallback);

        Encoding encoding;
        var offset = 0;
        var usedFallback = false;

        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            encoding = new UTF8Encoding(false);
            offset = 3;
        }
        else if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
        {
            encoding = Encoding.Unicode;
            offset = 2;
        }
        else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
        {
            encoding = Encoding.BigEndianUnicode;
            offset = 2;
        }
        else if (IsValidUtf8(bytes))
        {
            encoding = new UTF8Encoding(false);
        }
        else
        {
            encoding = fallback;
            usedFallback = true;
        }

        var text = encoding.GetString(bytes, offset, bytes.Length - offset);

        // Normalise line endings and drop the DOS end-of-file marker C/SIDE sometimes appends.
        text = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').TrimEnd('\u001a');
        return new DecodedText(text, encoding, usedFallback);
    }

    private static bool IsValidUtf8(byte[] bytes)
    {
        try
        {
            _ = new UTF8Encoding(false, throwOnInvalidBytes: true).GetCharCount(bytes);
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }
}
