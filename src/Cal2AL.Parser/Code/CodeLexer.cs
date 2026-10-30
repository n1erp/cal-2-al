namespace Cal2AL.Parser.Code;

/// <summary>Which language rules the lexer applies.</summary>
public enum CodeLanguage
{
    /// <summary>C/AL: <c>{ }</c> comments (nesting), no <c>/* */</c>.</summary>
    CAL,

    /// <summary>AL: <c>/* */</c> comments; <c>{</c> is not a comment.</summary>
    AL,
}

/// <summary>
/// Splits C/AL or AL code into tokens, keeping whitespace, line breaks and comments as trivia tokens so the
/// original text can be reproduced exactly (concatenating all token texts gives back the input).
/// </summary>
public static class CodeLexer
{
    public static List<CodeToken> Tokenize(string text, CodeLanguage language = CodeLanguage.CAL, int firstLine = 1)
    {
        var tokens = new List<CodeToken>();
        var pos = 0;
        var line = firstLine;
        var lineStart = 0;

        while (pos < text.Length)
        {
            var start = pos;
            var startLine = line;
            var column = pos - lineStart + 1;
            var c = text[pos];
            CodeTokenKind kind;

            switch (c)
            {
                case ' ' or '\t' or '\f':
                    while (pos < text.Length && text[pos] is ' ' or '\t' or '\f')
                    {
                        pos++;
                    }

                    kind = CodeTokenKind.Whitespace;
                    break;

                case '\r' or '\n':
                    if (c == '\r' && pos + 1 < text.Length && text[pos + 1] == '\n')
                    {
                        pos++;
                    }

                    pos++;
                    line++;
                    lineStart = pos;
                    kind = CodeTokenKind.NewLine;
                    break;

                case '/' when Peek(text, pos + 1) == '/':
                    while (pos < text.Length && text[pos] is not ('\n' or '\r'))
                    {
                        pos++;
                    }

                    kind = CodeTokenKind.LineComment;
                    break;

                case '/' when language == CodeLanguage.AL && Peek(text, pos + 1) == '*':
                {
                    pos += 2;
                    while (pos < text.Length && !(text[pos] == '*' && Peek(text, pos + 1) == '/'))
                    {
                        if (text[pos] == '\n')
                        {
                            line++;
                            lineStart = pos + 1;
                        }

                        pos++;
                    }

                    pos = Math.Min(text.Length, pos + 2);
                    kind = CodeTokenKind.BlockComment;
                    break;
                }

                case '{' when language == CodeLanguage.CAL:
                {
                    var depth = 0;
                    while (pos < text.Length)
                    {
                        var ch = text[pos];
                        if (ch == '{')
                        {
                            depth++;
                        }
                        else if (ch == '}')
                        {
                            depth--;
                        }
                        else if (ch == '\n')
                        {
                            line++;
                            lineStart = pos + 1;
                        }

                        pos++;
                        if (depth == 0)
                        {
                            break;
                        }
                    }

                    kind = CodeTokenKind.BraceComment;
                    break;
                }

                case '\'':
                    pos++;
                    while (pos < text.Length)
                    {
                        if (text[pos] == '\'')
                        {
                            if (Peek(text, pos + 1) == '\'')
                            {
                                pos += 2;
                                continue;
                            }

                            pos++;
                            break;
                        }

                        if (text[pos] is '\n' or '\r')
                        {
                            break; // unterminated string; the parser reports it
                        }

                        pos++;
                    }

                    kind = CodeTokenKind.String;
                    break;

                case '"':
                    pos++;
                    while (pos < text.Length && text[pos] != '"' && text[pos] is not ('\n' or '\r'))
                    {
                        pos++;
                    }

                    if (pos < text.Length && text[pos] == '"')
                    {
                        pos++;
                    }

                    kind = CodeTokenKind.QuotedIdentifier;
                    break;

                case >= '0' and <= '9':
                    kind = LexNumber(text, ref pos);
                    break;

                case ':':
                    pos++;
                    if (Peek(text, pos) == '=')
                    {
                        pos++;
                        kind = CodeTokenKind.Assign;
                    }
                    else if (Peek(text, pos) == ':')
                    {
                        pos++;
                        kind = CodeTokenKind.DoubleColon;
                    }
                    else
                    {
                        kind = CodeTokenKind.Colon;
                    }

                    break;

                case '+':
                    kind = Compound(text, ref pos, CodeTokenKind.Plus, CodeTokenKind.PlusAssign);
                    break;
                case '-':
                    kind = Compound(text, ref pos, CodeTokenKind.Minus, CodeTokenKind.MinusAssign);
                    break;
                case '*':
                    kind = Compound(text, ref pos, CodeTokenKind.Multiply, CodeTokenKind.MultiplyAssign);
                    break;
                case '/':
                    kind = Compound(text, ref pos, CodeTokenKind.Divide, CodeTokenKind.DivideAssign);
                    break;

                case '<':
                    pos++;
                    if (Peek(text, pos) == '=')
                    {
                        pos++;
                        kind = CodeTokenKind.LessEqual;
                    }
                    else if (Peek(text, pos) == '>')
                    {
                        pos++;
                        kind = CodeTokenKind.NotEqual;
                    }
                    else
                    {
                        kind = CodeTokenKind.Less;
                    }

                    break;

                case '>':
                    kind = Compound(text, ref pos, CodeTokenKind.Greater, CodeTokenKind.GreaterEqual);
                    break;

                case '=':
                    pos++;
                    kind = CodeTokenKind.Equal;
                    break;
                case ';':
                    pos++;
                    kind = CodeTokenKind.Semicolon;
                    break;
                case ',':
                    pos++;
                    kind = CodeTokenKind.Comma;
                    break;
                case '.':
                    pos++;
                    if (Peek(text, pos) == '.')
                    {
                        pos++;
                        kind = CodeTokenKind.DotDot;
                    }
                    else
                    {
                        kind = CodeTokenKind.Dot;
                    }

                    break;
                case '(':
                    pos++;
                    kind = CodeTokenKind.OpenParen;
                    break;
                case ')':
                    pos++;
                    kind = CodeTokenKind.CloseParen;
                    break;
                case '[':
                    pos++;
                    kind = CodeTokenKind.OpenBracket;
                    break;
                case ']':
                    pos++;
                    kind = CodeTokenKind.CloseBracket;
                    break;

                default:
                    if (char.IsLetter(c) || c == '_')
                    {
                        while (pos < text.Length && (char.IsLetterOrDigit(text[pos]) || text[pos] == '_'))
                        {
                            pos++;
                        }

                        kind = CodeTokenKind.Identifier;
                    }
                    else
                    {
                        pos++;
                        kind = CodeTokenKind.Unknown;
                    }

                    break;
            }

            tokens.Add(new CodeToken(kind, text[start..pos], start, startLine, column));
        }

        tokens.Add(new CodeToken(CodeTokenKind.EndOfInput, string.Empty, text.Length, line, text.Length - lineStart + 1));
        return tokens;
    }

    private static char Peek(string text, int index) => index < text.Length ? text[index] : '\0';

    private static CodeTokenKind Compound(string text, ref int pos, CodeTokenKind single, CodeTokenKind withEquals)
    {
        pos++;
        if (Peek(text, pos) == '=')
        {
            pos++;
            return withEquals;
        }

        return single;
    }

    /// <summary>Integer, decimal, BigInteger (<c>L</c>), date (<c>D</c>), time (<c>T</c>) and datetime (<c>DT</c>) literals.</summary>
    private static CodeTokenKind LexNumber(string text, ref int pos)
    {
        while (pos < text.Length && char.IsDigit(text[pos]))
        {
            pos++;
        }

        var kind = CodeTokenKind.Integer;
        if (Peek(text, pos) == '.' && char.IsDigit(Peek(text, pos + 1)))
        {
            pos++;
            while (pos < text.Length && char.IsDigit(text[pos]))
            {
                pos++;
            }

            kind = CodeTokenKind.Decimal;
        }

        var suffix = char.ToUpperInvariant(Peek(text, pos));
        var next = Peek(text, pos + 1);
        if (suffix == 'D' && char.ToUpperInvariant(next) == 'T' && !IsWordChar(Peek(text, pos + 2)))
        {
            pos += 2;
            return CodeTokenKind.DateTime;
        }

        if (!IsWordChar(next))
        {
            switch (suffix)
            {
                case 'D' when kind == CodeTokenKind.Integer:
                    pos++;
                    return CodeTokenKind.Date;
                case 'T':
                    pos++;
                    return CodeTokenKind.Time;
                case 'L' when kind == CodeTokenKind.Integer:
                    pos++;
                    return CodeTokenKind.BigInteger;
            }
        }

        return kind;
    }

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';
}
