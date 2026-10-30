namespace Cal2AL.Parser.Code;

/// <summary>Thrown when code does not follow C/AL syntax.</summary>
public sealed class CodeParseException(string message, CodeToken token) : Exception(message)
{
    public CodeToken Token { get; } = token;
}

/// <summary>
/// Recursive-descent parser for C/AL statements and expressions (the language inside BEGIN … END).
/// It is used to validate code, to find constructs that need attention, and to count statements and database
/// operations before and after conversion. It accepts AL syntax too, so generated bodies can be re-parsed.
/// </summary>
public sealed class CodeParser
{
    private static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "AND", "ASSERTERROR", "BEGIN", "CASE", "DIV", "DO", "DOWNTO", "ELSE", "END", "EXIT", "FALSE", "FOR", "FOREACH",
        "IF", "IN", "MOD", "NOT", "OF", "OR", "REPEAT", "THEN", "TO", "TRUE", "UNTIL", "WHILE", "WITH", "XOR",
    };

    private readonly List<CodeToken> tokens;
    private readonly List<CodeToken> significant;
    private int index;

    private CodeParser(List<CodeToken> tokens)
    {
        this.tokens = tokens;
        significant = tokens.Where(t => !t.IsTrivia).ToList();
    }

    public static bool IsReservedWord(string word) => Keywords.Contains(word);

    /// <summary>Parses a body (the statement list between BEGIN and END).</summary>
    public static CodeBodySyntax ParseBody(string text, CodeLanguage language = CodeLanguage.CAL, int firstLine = 1)
    {
        var tokens = CodeLexer.Tokenize(text, language, firstLine);
        return ParseBody(tokens);
    }

    public static CodeBodySyntax ParseBody(List<CodeToken> tokens)
    {
        var parser = new CodeParser(tokens);
        var statements = parser.ParseStatementList(static t => t.Kind == CodeTokenKind.EndOfInput);
        parser.Expect(CodeTokenKind.EndOfInput, "end of code");
        return new CodeBodySyntax { Statements = statements, Tokens = tokens };
    }

    // ---- token helpers ---------------------------------------------------------------------------

    private CodeToken Current => significant[Math.Min(index, significant.Count - 1)];

    private CodeToken PeekToken(int offset = 1) => significant[Math.Min(index + offset, significant.Count - 1)];

    private CodeToken Previous => significant[Math.Max(0, index - 1)];

    private CodeToken Advance()
    {
        var t = Current;
        if (index < significant.Count - 1)
        {
            index++;
        }

        return t;
    }

    private bool AtKeyword(string keyword) => Current.Is(keyword);

    private CodeToken ExpectKeyword(string keyword)
    {
        if (!AtKeyword(keyword))
        {
            throw Error($"'{keyword}' expected but found '{Describe(Current)}'.");
        }

        return Advance();
    }

    private CodeToken Expect(CodeTokenKind kind, string what)
    {
        if (Current.Kind != kind)
        {
            throw Error($"{what} expected but found '{Describe(Current)}'.");
        }

        return Advance();
    }

    private static string Describe(CodeToken token) => token.Kind == CodeTokenKind.EndOfInput ? "end of code" : token.Text;

    private CodeParseException Error(string message) => new(message, Current);

    // ---- statements --------------------------------------------------------------------------------

    /// <summary>Parses <c>stmt { ; stmt }</c> until <paramref name="atEnd"/> is true for the current token.</summary>
    private List<StatementSyntax> ParseStatementList(Func<CodeToken, bool> atEnd)
    {
        var list = new List<StatementSyntax>();
        while (true)
        {
            if (atEnd(Current))
            {
                return list;
            }

            var statement = ParseStatement();
            list.Add(statement);

            if (Current.Kind == CodeTokenKind.Semicolon)
            {
                Advance();
                continue;
            }

            if (atEnd(Current))
            {
                return list;
            }

            throw Error($"';' expected but found '{Describe(Current)}'.");
        }
    }

    private StatementSyntax ParseStatement()
    {
        var first = Current;
        if (first.Kind is CodeTokenKind.Semicolon || first.Is("END") || first.Is("UNTIL") || first.Is("ELSE") || first.Kind == CodeTokenKind.EndOfInput)
        {
            return new EmptyStatement { FirstToken = first, LastToken = first };
        }

        if (first.Kind == CodeTokenKind.Identifier)
        {
            switch (first.Text.ToUpperInvariant())
            {
                case "BEGIN": return ParseBlock();
                case "IF": return ParseIf();
                case "CASE": return ParseCase();
                case "WHILE": return ParseWhile();
                case "REPEAT": return ParseRepeat();
                case "FOR": return ParseFor();
                case "FOREACH": return ParseForEach();
                case "WITH": return ParseWith();
                case "EXIT": return ParseExit();
                case "ASSERTERROR":
                {
                    Advance();
                    var inner = ParseStatement();
                    return new AssertErrorStatement { FirstToken = first, LastToken = inner.LastToken, Statement = inner };
                }
            }
        }

        var target = ParseExpression();
        if (Current.Kind is CodeTokenKind.Assign or CodeTokenKind.PlusAssign or CodeTokenKind.MinusAssign
            or CodeTokenKind.MultiplyAssign or CodeTokenKind.DivideAssign)
        {
            var op = Advance();
            var value = ParseExpression();
            return new AssignmentStatement { FirstToken = first, LastToken = value.LastToken, Target = target, Operator = op, Value = value };
        }

        return new ExpressionStatement { FirstToken = first, LastToken = target.LastToken, Expression = target };
    }

    private BlockStatement ParseBlock()
    {
        var begin = ExpectKeyword("BEGIN");
        var statements = ParseStatementList(t => t.Is("END") || t.Kind == CodeTokenKind.EndOfInput);
        var end = ExpectKeyword("END");
        return new BlockStatement { FirstToken = begin, LastToken = end, Statements = statements };
    }

    private IfStatement ParseIf()
    {
        var first = ExpectKeyword("IF");
        var condition = ParseExpression();
        ExpectKeyword("THEN");
        var then = ParseStatement();
        StatementSyntax? elseStatement = null;
        if (AtKeyword("ELSE"))
        {
            Advance();
            elseStatement = ParseStatement();
        }

        return new IfStatement
        {
            FirstToken = first,
            LastToken = (elseStatement ?? then).LastToken,
            Condition = condition,
            Then = then,
            Else = elseStatement,
        };
    }

    private CaseStatement ParseCase()
    {
        var first = ExpectKeyword("CASE");
        var selector = ParseExpression();
        ExpectKeyword("OF");
        var branches = new List<CaseBranch>();
        List<StatementSyntax>? elseStatements = null;

        while (true)
        {
            if (Current.Kind == CodeTokenKind.Semicolon)
            {
                Advance();
                continue;
            }

            if (AtKeyword("END") || Current.Kind == CodeTokenKind.EndOfInput)
            {
                break;
            }

            if (AtKeyword("ELSE"))
            {
                Advance();
                elseStatements = ParseStatementList(t => t.Is("END") || t.Kind == CodeTokenKind.EndOfInput);
                break;
            }

            var labelStart = Current;
            var labels = new List<ExpressionSyntax> { ParseCaseLabel() };
            while (Current.Kind == CodeTokenKind.Comma)
            {
                Advance();
                labels.Add(ParseCaseLabel());
            }

            Expect(CodeTokenKind.Colon, "':' after CASE label");
            var body = ParseStatement();
            branches.Add(new CaseBranch { FirstToken = labelStart, LastToken = body.LastToken, Labels = labels, Body = body });

            if (Current.Kind == CodeTokenKind.Semicolon)
            {
                Advance();
            }
            else if (!AtKeyword("END") && !AtKeyword("ELSE"))
            {
                throw Error($"';' expected after CASE branch but found '{Describe(Current)}'.");
            }
        }

        var end = ExpectKeyword("END");
        return new CaseStatement { FirstToken = first, LastToken = end, Selector = selector, Branches = branches, ElseStatements = elseStatements };
    }

    private ExpressionSyntax ParseCaseLabel()
    {
        var first = Current;
        if (Current.Kind == CodeTokenKind.DotDot)
        {
            Advance();
            var high = ParseExpression();
            return new RangeExpression { FirstToken = first, LastToken = high.LastToken, High = high };
        }

        var low = ParseExpression();
        if (Current.Kind == CodeTokenKind.DotDot)
        {
            var dots = Advance();
            if (Current.Kind is CodeTokenKind.Colon or CodeTokenKind.Comma)
            {
                return new RangeExpression { FirstToken = first, LastToken = dots, Low = low };
            }

            var high = ParseExpression();
            return new RangeExpression { FirstToken = first, LastToken = high.LastToken, Low = low, High = high };
        }

        return low;
    }

    private WhileStatement ParseWhile()
    {
        var first = ExpectKeyword("WHILE");
        var condition = ParseExpression();
        ExpectKeyword("DO");
        var body = ParseStatement();
        return new WhileStatement { FirstToken = first, LastToken = body.LastToken, Condition = condition, Body = body };
    }

    private RepeatStatement ParseRepeat()
    {
        var first = ExpectKeyword("REPEAT");
        var statements = ParseStatementList(t => t.Is("UNTIL") || t.Kind == CodeTokenKind.EndOfInput);
        ExpectKeyword("UNTIL");
        var condition = ParseExpression();
        return new RepeatStatement { FirstToken = first, LastToken = condition.LastToken, Statements = statements, Condition = condition };
    }

    private ForStatement ParseFor()
    {
        var first = ExpectKeyword("FOR");
        var variable = ParsePostfix();
        Expect(CodeTokenKind.Assign, "':=' in FOR statement");
        var from = ParseExpression();
        bool downTo;
        if (AtKeyword("TO"))
        {
            downTo = false;
        }
        else if (AtKeyword("DOWNTO"))
        {
            downTo = true;
        }
        else
        {
            throw Error($"TO or DOWNTO expected but found '{Describe(Current)}'.");
        }

        Advance();
        var to = ParseExpression();
        ExpectKeyword("DO");
        var body = ParseStatement();
        return new ForStatement { FirstToken = first, LastToken = body.LastToken, Variable = variable, From = from, DownTo = downTo, To = to, Body = body };
    }

    private ForEachStatement ParseForEach()
    {
        var first = ExpectKeyword("FOREACH");
        var variable = ParsePostfix();
        ExpectKeyword("IN");
        var collection = ParseExpression();
        ExpectKeyword("DO");
        var body = ParseStatement();
        return new ForEachStatement { FirstToken = first, LastToken = body.LastToken, Variable = variable, Collection = collection, Body = body };
    }

    private WithStatement ParseWith()
    {
        var first = ExpectKeyword("WITH");
        var record = ParseExpression();
        ExpectKeyword("DO");
        var body = ParseStatement();
        return new WithStatement { FirstToken = first, LastToken = body.LastToken, Record = record, Body = body };
    }

    private ExitStatement ParseExit()
    {
        var first = ExpectKeyword("EXIT");
        if (Current.Kind == CodeTokenKind.OpenParen)
        {
            Advance();
            ExpressionSyntax? value = null;
            if (Current.Kind != CodeTokenKind.CloseParen)
            {
                value = ParseExpression();
            }

            var close = Expect(CodeTokenKind.CloseParen, "')'");
            return new ExitStatement { FirstToken = first, LastToken = close, Value = value };
        }

        return new ExitStatement { FirstToken = first, LastToken = first };
    }

    // ---- expressions -------------------------------------------------------------------------------

    /// <summary>Relational level: = &lt;&gt; &lt; &lt;= &gt; &gt;= IN (lowest precedence).</summary>
    public ExpressionSyntax ParseExpression()
    {
        var left = ParseAdditive();
        while (Current.Kind is CodeTokenKind.Equal or CodeTokenKind.NotEqual or CodeTokenKind.Less
                   or CodeTokenKind.LessEqual or CodeTokenKind.Greater or CodeTokenKind.GreaterEqual
               || AtKeyword("IN"))
        {
            var op = Advance();
            var right = ParseAdditive();
            left = new BinaryExpression { FirstToken = left.FirstToken, LastToken = right.LastToken, Left = left, Operator = op, Right = right };
        }

        return left;
    }

    private ExpressionSyntax ParseAdditive()
    {
        var left = ParseMultiplicative();
        while (Current.Kind is CodeTokenKind.Plus or CodeTokenKind.Minus || AtKeyword("OR") || AtKeyword("XOR"))
        {
            var op = Advance();
            var right = ParseMultiplicative();
            left = new BinaryExpression { FirstToken = left.FirstToken, LastToken = right.LastToken, Left = left, Operator = op, Right = right };
        }

        return left;
    }

    private ExpressionSyntax ParseMultiplicative()
    {
        var left = ParseUnary();
        while (Current.Kind is CodeTokenKind.Multiply or CodeTokenKind.Divide || AtKeyword("DIV") || AtKeyword("MOD") || AtKeyword("AND"))
        {
            var op = Advance();
            var right = ParseUnary();
            left = new BinaryExpression { FirstToken = left.FirstToken, LastToken = right.LastToken, Left = left, Operator = op, Right = right };
        }

        return left;
    }

    private ExpressionSyntax ParseUnary()
    {
        if (AtKeyword("NOT") || Current.Kind is CodeTokenKind.Minus or CodeTokenKind.Plus)
        {
            var op = Advance();
            var operand = ParseUnary();
            return new UnaryExpression { FirstToken = op, LastToken = operand.LastToken, Operator = op, Operand = operand };
        }

        return ParsePostfix();
    }

    private ExpressionSyntax ParsePostfix()
    {
        var expression = ParsePrimary();
        while (true)
        {
            switch (Current.Kind)
            {
                case CodeTokenKind.Dot:
                {
                    Advance();
                    var name = ExpectMemberName();
                    expression = new MemberAccessExpression { FirstToken = expression.FirstToken, LastToken = name, Target = expression, Name = name };
                    continue;
                }

                case CodeTokenKind.DoubleColon:
                {
                    Advance();
                    var name = ExpectMemberName();
                    expression = new OptionAccessExpression { FirstToken = expression.FirstToken, LastToken = name, Target = expression, Name = name };
                    continue;
                }

                case CodeTokenKind.OpenParen:
                {
                    Advance();
                    var args = new List<ExpressionSyntax>();
                    if (Current.Kind != CodeTokenKind.CloseParen)
                    {
                        args.Add(ParseExpression());
                        while (Current.Kind == CodeTokenKind.Comma)
                        {
                            Advance();
                            args.Add(ParseExpression());
                        }
                    }

                    var close = Expect(CodeTokenKind.CloseParen, "')'");
                    expression = new InvocationExpression { FirstToken = expression.FirstToken, LastToken = close, Target = expression, Arguments = args };
                    continue;
                }

                case CodeTokenKind.OpenBracket:
                {
                    Advance();
                    var indices = new List<ExpressionSyntax> { ParseExpression() };
                    while (Current.Kind == CodeTokenKind.Comma)
                    {
                        Advance();
                        indices.Add(ParseExpression());
                    }

                    var close = Expect(CodeTokenKind.CloseBracket, "']'");
                    expression = new IndexExpression { FirstToken = expression.FirstToken, LastToken = close, Target = expression, Indices = indices };
                    continue;
                }
            }

            return expression;
        }
    }

    /// <summary>After '.' or '::' any identifier is allowed, including words that are keywords elsewhere.</summary>
    private CodeToken ExpectMemberName()
    {
        if (Current.IsIdentifier || Current.Kind == CodeTokenKind.Integer)
        {
            return Advance();
        }

        throw Error($"Member name expected but found '{Describe(Current)}'.");
    }

    private ExpressionSyntax ParsePrimary()
    {
        var token = Current;
        switch (token.Kind)
        {
            case CodeTokenKind.Integer or CodeTokenKind.BigInteger or CodeTokenKind.Decimal or CodeTokenKind.String
                or CodeTokenKind.Date or CodeTokenKind.Time or CodeTokenKind.DateTime:
                if (token.Kind == CodeTokenKind.String && (token.Text.Length < 2 || token.Text[^1] != '\''))
                {
                    throw Error("Unterminated string literal.");
                }

                Advance();
                return new LiteralExpression { FirstToken = token, LastToken = token, Token = token };

            case CodeTokenKind.QuotedIdentifier:
                Advance();
                return new NameExpression { FirstToken = token, LastToken = token, Name = token };

            case CodeTokenKind.Identifier:
                if (token.Is("TRUE") || token.Is("FALSE"))
                {
                    Advance();
                    return new LiteralExpression { FirstToken = token, LastToken = token, Token = token };
                }

                if (Keywords.Contains(token.Text))
                {
                    throw Error($"Expression expected but found keyword '{token.Text}'.");
                }

                Advance();
                return new NameExpression { FirstToken = token, LastToken = token, Name = token };

            case CodeTokenKind.OpenParen:
            {
                Advance();
                var inner = ParseExpression();
                var close = Expect(CodeTokenKind.CloseParen, "')'");
                return new ParenthesizedExpression { FirstToken = token, LastToken = close, Inner = inner };
            }

            case CodeTokenKind.OpenBracket:
            {
                Advance();
                var elements = new List<ExpressionSyntax>();
                if (Current.Kind != CodeTokenKind.CloseBracket)
                {
                    elements.Add(ParseSetElement());
                    while (Current.Kind == CodeTokenKind.Comma)
                    {
                        Advance();
                        elements.Add(ParseSetElement());
                    }
                }

                var close = Expect(CodeTokenKind.CloseBracket, "']'");
                return new SetExpression { FirstToken = token, LastToken = close, Elements = elements };
            }
        }

        throw Error($"Expression expected but found '{Describe(token)}'.");
    }

    private ExpressionSyntax ParseSetElement()
    {
        var first = Current;
        if (Current.Kind == CodeTokenKind.DotDot)
        {
            Advance();
            var high = ParseAdditive();
            return new RangeExpression { FirstToken = first, LastToken = high.LastToken, High = high };
        }

        // Set elements are full expressions: TRUE IN [A <> B, NOT (C OR D)] is valid C/AL.
        var low = ParseExpression();
        if (Current.Kind == CodeTokenKind.DotDot)
        {
            var dots = Advance();
            if (Current.Kind is CodeTokenKind.Comma or CodeTokenKind.CloseBracket)
            {
                return new RangeExpression { FirstToken = first, LastToken = dots, Low = low };
            }

            var high = ParseAdditive();
            return new RangeExpression { FirstToken = first, LastToken = high.LastToken, Low = low, High = high };
        }

        return low;
    }
}
