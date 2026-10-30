using Cal2AL.Converter;
using Cal2AL.Converter.Objects;
using Cal2AL.Converter.Symbols;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Generator;
using Cal2AL.Parser.Txt;

namespace Cal2AL.Tests.Converter;

public class CodeConversionTests
{
    /// <summary>Wraps a body in a minimal codeunit, converts it and returns the AL text and diagnostics.</summary>
    private static (string Al, List<Diagnostic> Diagnostics, ObjectContext Context) ConvertCodeunit(string variables, string body, ConversionOptions? options = null)
    {
        var text = string.Join("\n",
            "OBJECT Codeunit 50900 Test Codeunit",
            "{",
            "  OBJECT-PROPERTIES",
            "  {",
            "  }",
            "  PROPERTIES",
            "  {",
            "  }",
            "  CODE",
            "  {",
            variables.Length == 0 ? string.Empty : "    VAR\n" + variables,
            string.Empty,
            "    PROCEDURE Run@1();",
            "    BEGIN",
            string.Join("\n", body.Split('\n').Select(l => "      " + l)),
            "    END;",
            string.Empty,
            "    BEGIN",
            "    END.",
            "  }",
            "}",
            string.Empty);
        var parsed = new TxtFileParser().ParseText(text, "test.txt");
        Assert.Empty(parsed.Failures);
        var obj = Assert.Single(parsed.Objects);
        var context = new ObjectContext(new RunContext(options ?? new ConversionOptions(), SymbolTable.Build([obj])), obj);
        var output = new CodeunitConverter().Convert(context);
        return (new AlWriter(new AlWriterOptions { NewLine = "\n" }).Render(output.AlObject!), context.Diagnostics, context);
    }

    [Fact]
    public void Brace_comments_become_block_comments_and_code_is_unchanged()
    {
        var (al, diagnostics, context) = ConvertCodeunit("", "{ check the { nested } note }\nIF x = 1 THEN\n  EXIT;");
        Assert.Contains("/* check the { nested } note */", al);
        Assert.Contains("IF x = 1 THEN", al);
        Assert.DoesNotContain(diagnostics, d => d.Code == DiagnosticDescriptors.PreservationCheckFailed.Code);
        Assert.Equal(context.Statements, context.OutputStatements);
    }

    [Fact]
    public void Comment_containing_block_comment_end_is_adjusted_and_reported()
    {
        var (al, diagnostics, _) = ConvertCodeunit("", "{ a */ b }\nEXIT;");
        Assert.DoesNotContain("a */ b */", al);
        Assert.Contains(diagnostics, d => d.Code == DiagnosticDescriptors.CommentAdjusted.Code);
    }

    [Fact]
    public void Date_constants_are_rewritten_to_AL_format_and_reported()
    {
        var (al, diagnostics, _) = ConvertCodeunit("      D@1000 : Date;\n", "D := 123125D;\nIF D = 0D THEN;");
        Assert.Contains("D := 20251231D;", al);
        Assert.Contains("IF D = 0D THEN;", al);
        Assert.Contains(diagnostics, d => d.Code == DiagnosticDescriptors.DateConstant.Code);
    }

    [Fact]
    public void Day_month_year_option_changes_date_interpretation()
    {
        var (al, _, _) = ConvertCodeunit("      D@1000 : Date;\n", "D := 311225D;", new ConversionOptions { CalDateOrder = Cal2AL.Parser.Code.CalDateOrder.DayMonthYear });
        Assert.Contains("D := 20251231D;", al);
    }

    [Fact]
    public void Identifiers_that_are_AL_keywords_are_quoted_in_declarations_and_code()
    {
        var (al, diagnostics, _) = ConvertCodeunit("      Key@1000 : Integer;\n", "Key := 1;\nKey += Key;");
        Assert.Contains("\"Key\": Integer;", al);
        Assert.Contains("\"Key\" := 1;", al);
        Assert.Contains("\"Key\" += \"Key\";", al);
        Assert.Contains(diagnostics, d => d.Code == DiagnosticDescriptors.ReservedIdentifierQuoted.Code);
    }

    [Fact]
    public void Unsupported_functions_are_kept_and_reported_as_errors()
    {
        var (al, diagnostics, _) = ConvertCodeunit("", "IF ISSERVICETIER THEN\n  SHELL('notepad.exe');");
        Assert.Contains("SHELL('notepad.exe');", al);
        Assert.Equal(2, diagnostics.Count(d => d.Code == DiagnosticDescriptors.UnsupportedFunction.Code && d.Severity == DiagnosticSeverity.Error));
    }

    [Fact]
    public void Code_that_cannot_be_parsed_is_still_carried_over_with_an_error()
    {
        var (al, diagnostics, _) = ConvertCodeunit("", "IF THEN\n  x := 1;");
        Assert.Contains("IF THEN", al);
        Assert.Contains("x := 1;", al);
        var error = Assert.Single(diagnostics, d => d.Code == DiagnosticDescriptors.CodeSyntaxError.Code);
        Assert.Equal("Run", error.Location.Member);
    }

    [Fact]
    public void Every_statement_and_database_operation_is_preserved()
    {
        var (_, diagnostics, context) = ConvertCodeunit(
            "      Customer@1000 : Record 50000;\n      Total@1001 : Decimal;\n",
            "IF Customer.FINDSET THEN\n  REPEAT\n    Customer.CALCFIELDS(Balance);\n    Total += Customer.Balance;\n  UNTIL Customer.NEXT = 0;");
        Assert.True(context.Statements > 0);
        Assert.Equal(context.Statements, context.OutputStatements);
        Assert.Equal(3, context.DatabaseOperations);
        Assert.Equal(context.DatabaseOperations, context.OutputDatabaseOperations);
        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Unresolved_record_references_keep_the_id_and_are_reported_once()
    {
        var (al, diagnostics, _) = ConvertCodeunit("      A@1000 : Record 18;\n      B@1001 : Record 18;\n", "A.GET;\nB.GET;");
        Assert.Contains("A: Record 18;", al);
        Assert.Single(diagnostics, d => d.Code == DiagnosticDescriptors.UnresolvedObjectReference.Code);
    }

    [Fact]
    public void Object_types_parse_case_insensitively()
    {
        Assert.Equal(CalObjectType.XmlPort, CalObjectTypeExtensions.ParseObjectType("XMLport"));
        Assert.Equal(CalObjectType.Codeunit, CalObjectTypeExtensions.ParseObjectType("codeunit"));
    }
}
