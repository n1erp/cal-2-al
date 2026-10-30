using Cal2AL.Parser.Code;

namespace Cal2AL.Tests.Parser;

public class CodeLexerAndParserTests
{
    [Fact]
    public void Brace_comments_nest_in_CAL()
    {
        var tokens = CodeLexer.Tokenize("a := 1; { outer { inner } still comment } b := 2;");
        var comment = Assert.Single(tokens, t => t.Kind == CodeTokenKind.BraceComment);
        Assert.Equal("{ outer { inner } still comment }", comment.Text);
        Assert.Contains(tokens, t => t.Text == "b");
    }

    [Fact]
    public void Braces_are_not_comments_in_AL()
    {
        var tokens = CodeLexer.Tokenize("a /* x */ { b }", CodeLanguage.AL);
        Assert.Contains(tokens, t => t.Kind == CodeTokenKind.BlockComment && t.Text == "/* x */");
        Assert.DoesNotContain(tokens, t => t.Kind == CodeTokenKind.BraceComment);
    }

    [Theory]
    [InlineData("'It''s'", CodeTokenKind.String)]
    [InlineData("010125D", CodeTokenKind.Date)]
    [InlineData("0D", CodeTokenKind.Date)]
    [InlineData("235959T", CodeTokenKind.Time)]
    [InlineData("0DT", CodeTokenKind.DateTime)]
    [InlineData("12.5", CodeTokenKind.Decimal)]
    [InlineData("\"Ship-to Code\"", CodeTokenKind.QuotedIdentifier)]
    public void Recognises_literals(string text, CodeTokenKind kind)
    {
        var token = Assert.Single(CodeLexer.Tokenize(text), t => !t.IsTrivia && t.Kind != CodeTokenKind.EndOfInput);
        Assert.Equal(kind, token.Kind);
        Assert.Equal(text, token.Text);
    }

    [Fact]
    public void Lexing_round_trips_the_text_exactly()
    {
        const string code = "IF Customer.FINDSET THEN\n  REPEAT\n    Total += Customer.Balance; // sum\n  UNTIL Customer.NEXT = 0;";
        var tokens = CodeLexer.Tokenize(code);
        Assert.Equal(code, string.Concat(tokens.Select(t => t.Text)));
    }

    [Fact]
    public void Counts_statements_and_database_operations_in_nested_code()
    {
        const string code = """
            IF Customer.FINDSET THEN
              REPEAT
                CASE Customer."Customer Type" OF
                  1:
                    BEGIN
                      Customer.CALCFIELDS(Balance);
                      Total := Total + Customer.Balance;
                    END;
                  ELSE
                    WHILE i < 10 DO
                      i += 1;
                END;
              UNTIL Customer.NEXT = 0;
            FOR i := 1 TO 3 DO
              WITH Customer DO
                MODIFY;
            """;
        var metrics = CodeMetrics.Compute(CodeParser.ParseBody(code));

        // IF, REPEAT, CASE, CALCFIELDS call, assignment, WHILE, +=, FOR, WITH, MODIFY call, BEGIN block.
        Assert.True(metrics.Statements >= 10, $"statements: {metrics.Statements}");
        Assert.Equal(4, metrics.DatabaseOperations); // FINDSET, CALCFIELDS, NEXT, MODIFY
    }

    [Fact]
    public void Reports_syntax_errors_with_position()
    {
        var ex = Assert.Throws<CodeParseException>(() => CodeParser.ParseBody("IF THEN\n  x := 1;"));
        Assert.Contains("THEN", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Token_comparison_treats_rewritten_dates_and_comments_as_equal()
    {
        var original = CodeLexer.Tokenize("d := 123125D; { note }", CodeLanguage.CAL);
        var converted = CodeLexer.Tokenize("d := 20251231D; /* note */", CodeLanguage.AL);
        Assert.Null(TokenStreamComparer.FindDifference(original, converted));
    }

    [Fact]
    public void Token_comparison_reports_a_changed_statement()
    {
        var original = CodeLexer.Tokenize("x := 1;", CodeLanguage.CAL);
        var converted = CodeLexer.Tokenize("x := 2;", CodeLanguage.AL);
        Assert.NotNull(TokenStreamComparer.FindDifference(original, converted));
    }
}
