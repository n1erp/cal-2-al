namespace Cal2AL.Parser.Code;

public enum CodeTokenKind
{
    EndOfInput,
    Whitespace,
    NewLine,

    /// <summary><c>// …</c> up to the end of the line.</summary>
    LineComment,

    /// <summary>C/AL <c>{ … }</c> comment (may nest).</summary>
    BraceComment,

    /// <summary>AL <c>/* … */</c> comment (only recognised in AL mode).</summary>
    BlockComment,

    Identifier,
    QuotedIdentifier,
    Integer,
    BigInteger,
    Decimal,
    String,
    Date,
    Time,
    DateTime,

    // Punctuation and operators
    Assign,          // :=
    PlusAssign,      // +=
    MinusAssign,     // -=
    MultiplyAssign,  // *=
    DivideAssign,    // /=
    Plus,
    Minus,
    Multiply,
    Divide,
    Equal,
    NotEqual,
    Less,
    LessEqual,
    Greater,
    GreaterEqual,
    Colon,
    DoubleColon,
    Semicolon,
    Comma,
    Dot,
    DotDot,
    OpenParen,
    CloseParen,
    OpenBracket,
    CloseBracket,

    /// <summary>A character that is not valid in C/AL code.</summary>
    Unknown,
}

/// <summary>A lexical token of C/AL (or AL) code, with its exact text and position.</summary>
public sealed record CodeToken(CodeTokenKind Kind, string Text, int Start, int Line, int Column)
{
    public int End => Start + Text.Length;

    public bool IsTrivia => Kind is CodeTokenKind.Whitespace or CodeTokenKind.NewLine
        or CodeTokenKind.LineComment or CodeTokenKind.BraceComment or CodeTokenKind.BlockComment;

    public bool IsComment => Kind is CodeTokenKind.LineComment or CodeTokenKind.BraceComment or CodeTokenKind.BlockComment;

    public bool IsIdentifier => Kind is CodeTokenKind.Identifier or CodeTokenKind.QuotedIdentifier;

    /// <summary>Identifier text without surrounding double quotes.</summary>
    public string IdentifierName => Kind == CodeTokenKind.QuotedIdentifier && Text.Length >= 2 && Text[^1] == '"'
        ? Text[1..^1]
        : Text;

    /// <summary>True for an unquoted identifier equal to <paramref name="keyword"/> (case-insensitive).</summary>
    public bool Is(string keyword) => Kind == CodeTokenKind.Identifier && string.Equals(Text, keyword, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => $"{Kind} '{Text}' @{Line}:{Column}";
}
