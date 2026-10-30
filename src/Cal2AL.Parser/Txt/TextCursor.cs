using Cal2AL.Core.Model;

namespace Cal2AL.Parser.Txt;

/// <summary>Thrown when an object's TXT text does not follow the C/SIDE format.</summary>
public sealed class TxtParseException(string message, int line, int column) : Exception(message)
{
    public int Line { get; } = line;
    public int Column { get; } = column;
}

/// <summary>
/// A character cursor over the text of one object with line/column tracking. Line numbers are reported
/// relative to the original file by adding <see cref="LineOffset"/>.
/// </summary>
internal sealed class TextCursor
{
    private readonly string text;
    private int pos;
    private int line = 1;
    private int lineStart;

    public TextCursor(string text, int lineOffset)
    {
        this.text = text;
        LineOffset = lineOffset;
    }

    /// <summary>Number of file lines before the first line of <see cref="text"/>.</summary>
    public int LineOffset { get; }

    public bool AtEnd => pos >= text.Length;

    public int Position => pos;

    /// <summary>1-based line number in the original file.</summary>
    public int Line => line + LineOffset;

    /// <summary>1-based column of the current character.</summary>
    public int Column => pos - lineStart + 1;

    public SourcePosition SourcePosition => new(Line, Column);

    public char Peek(int offset = 0)
    {
        var i = pos + offset;
        return i < text.Length ? text[i] : '\0';
    }

    public char Next()
    {
        if (pos >= text.Length)
        {
            return '\0';
        }

        var c = text[pos++];
        if (c == '\n')
        {
            line++;
            lineStart = pos;
        }

        return c;
    }

    /// <summary>Saved cursor state for look-ahead.</summary>
    public readonly record struct Mark(int Pos, int Line, int LineStart);

    public Mark Save() => new(pos, line, lineStart);

    public void Restore(Mark mark)
    {
        pos = mark.Pos;
        line = mark.Line;
        lineStart = mark.LineStart;
    }

    public void Advance(int count)
    {
        for (var i = 0; i < count; i++)
        {
            Next();
        }
    }

    public string Substring(int start, int end) => text[start..end];

    /// <summary>Skips spaces and tabs on the current line.</summary>
    public void SkipSpaces()
    {
        while (Peek() is ' ' or '\t' or '\f')
        {
            Next();
        }
    }

    /// <summary>Skips spaces, tabs and line breaks.</summary>
    public void SkipWhitespace()
    {
        while (Peek() is ' ' or '\t' or '\f' or '\n' or '\r')
        {
            Next();
        }
    }

    public bool AtLineEnd => Peek() is '\n' or '\0';

    /// <summary>Returns the rest of the current line (without the line break) and moves to the next line.</summary>
    public string ReadLine()
    {
        var start = pos;
        while (!AtEnd && Peek() != '\n')
        {
            Next();
        }

        var result = text[start..pos];
        if (Peek() == '\n')
        {
            Next();
        }

        return result.TrimEnd('\r');
    }

    /// <summary>The full text of the line the cursor is on.</summary>
    public string CurrentLineText()
    {
        var end = text.IndexOf('\n', lineStart);
        return (end < 0 ? text[lineStart..] : text[lineStart..end]).TrimEnd('\r');
    }

    /// <summary>Indentation of the current line (number of leading spaces; a tab counts as one).</summary>
    public int CurrentLineIndent()
    {
        var i = lineStart;
        while (i < text.Length && text[i] is ' ' or '\t')
        {
            i++;
        }

        return i - lineStart;
    }

    /// <summary>True when the text at the cursor is <paramref name="word"/> (case-insensitive) followed by a non-word character.</summary>
    public bool LookingAtWord(string word)
    {
        if (pos + word.Length > text.Length)
        {
            return false;
        }

        if (string.Compare(text, pos, word, 0, word.Length, StringComparison.OrdinalIgnoreCase) != 0)
        {
            return false;
        }

        var after = Peek(word.Length);
        return !IsWordChar(after);
    }

    public bool TryReadWord(string word)
    {
        if (!LookingAtWord(word))
        {
            return false;
        }

        Advance(word.Length);
        return true;
    }

    public void ExpectWord(string word)
    {
        if (!TryReadWord(word))
        {
            throw Error($"'{word}' expected but found '{Describe()}'.");
        }
    }

    public void Expect(char c)
    {
        if (Peek() != c)
        {
            throw Error($"'{c}' expected but found '{Describe()}'.");
        }

        Next();
    }

    /// <summary>Reads letters, digits and underscores.</summary>
    public string ReadWord()
    {
        var start = pos;
        while (IsWordChar(Peek()))
        {
            Next();
        }

        return text[start..pos];
    }

    public static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    public string Describe()
    {
        if (AtEnd)
        {
            return "<end of object>";
        }

        var lineText = CurrentLineText();
        var col = Column - 1;
        var snippet = col < lineText.Length ? lineText[col..] : string.Empty;
        snippet = snippet.Trim();
        if (snippet.Length == 0)
        {
            return "<end of line>";
        }

        return snippet.Length > 40 ? snippet[..40] + "…" : snippet;
    }

    public TxtParseException Error(string message) => new(message, Line, Column);
}
