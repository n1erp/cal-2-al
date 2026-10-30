using Cal2AL.Converter.Objects;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;

namespace Cal2AL.Tests.Converter;

public class ModificationConverterTests
{
    private static CalCodeBody Body(params string[] lines) => new(lines.Select((l, i) => new CodeLine(l, i + 1)).ToList());

    [Fact]
    public void Appended_lines_are_split_from_the_unchanged_original()
    {
        var split = CodeSplit.Compute(Body("A;", "B;"), Body("A;", "B;", "C;"));
        Assert.NotNull(split);
        Assert.Empty(split!.Before);
        Assert.Equal(["C;"], split.After.Select(l => l.Text));
        Assert.Null(split.SafetyProblem(new HashSet<string>()));
    }

    [Fact]
    public void Prepended_and_appended_lines_are_both_found()
    {
        var split = CodeSplit.Compute(Body("A;"), Body("X;", "A;", "Y;"));
        Assert.Equal(["X;"], split!.Before.Select(l => l.Text));
        Assert.Equal(["Y;"], split.After.Select(l => l.Text));
    }

    [Fact]
    public void Changed_original_lines_cannot_be_split()
    {
        Assert.Null(CodeSplit.Compute(Body("A;", "B;"), Body("A;", "B2;", "C;")));
    }

    [Fact]
    public void Exit_in_the_original_makes_appending_unsafe()
    {
        var split = CodeSplit.Compute(Body("IF x THEN", "  EXIT;"), Body("IF x THEN", "  EXIT;", "C;"));
        Assert.Contains("EXIT", split!.SafetyProblem(new HashSet<string>()));
    }

    [Fact]
    public void Exit_in_prepended_code_is_unsafe()
    {
        var split = CodeSplit.Compute(Body("A;"), Body("IF x THEN", "  EXIT;", "A;"));
        Assert.NotNull(split!.SafetyProblem(new HashSet<string>()));
    }

    [Fact]
    public void Added_code_that_uses_original_locals_is_unsafe()
    {
        var split = CodeSplit.Compute(Body("Total := 1;"), Body("Total := 1;", "MESSAGE('%1',Total);"));
        Assert.Contains("Total", split!.SafetyProblem(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Total" }));
    }

    [Theory]
    [InlineData("\"Salesperson Code\"(Field 4)", "Salesperson Code", "Field", 4, AnchorMode.After)]
    [InlineData("Control 12", null, "Control", 12, AnchorMode.After)]
    [InlineData("AddFirst:ContentArea", "ContentArea", null, null, AnchorMode.First)]
    [InlineData("AddAfter:General", "General", null, null, AnchorMode.After)]
    public void Parses_element_references(string text, string? name, string? kind, int? id, AnchorMode mode)
    {
        var reference = ElementReference.Parse(text);
        Assert.Equal(name, reference.Name);
        Assert.Equal(kind, reference.Kind);
        Assert.Equal(id, reference.Id);
        Assert.Equal(mode, reference.Mode);
    }

    [Fact]
    public void Table_delta_becomes_a_table_extension_with_fields_variables_procedures_and_trigger()
    {
        var (run, sink) = TestEngine.Convert(TestPaths.Fixtures);
        var result = run.Objects.Single(o => o.Source.Type == CalObjectType.Modification && o.Source.Id == 50000 && o.Source.ModificationTarget!.Contains("Table"));
        var al = result.AlText!;

        Assert.Contains("tableextension 70000000 \"Cal2AL Test Customer Ext\" extends \"Cal2AL Test Customer\"", al);
        Assert.Contains("field(50000; \"Loyalty Level\"; Option)", al);
        Assert.Contains("LoyaltyMgt: Codeunit \"Cal2AL Posting Logic\";", al);
        Assert.Contains("procedure AddLoyaltyPoints(Points: Integer)", al);
        Assert.Contains("trigger OnModify()", al);
        Assert.Contains("IF \"Loyalty Points\" < 0 THEN", al);
        Assert.DoesNotContain("\"Last Date Modified\" := TODAY;", al); // the base object's own line is not duplicated
        Assert.Equal(ObjectConversionStatus.Converted, result.Result.Status);
        Assert.Contains(result.Result.OutputFiles, f => f.EndsWith(".TableExt.al", StringComparison.Ordinal));
        Assert.NotNull(sink.GetText(result.Result.OutputFiles[0]));
    }

    [Fact]
    public void Page_delta_converts_safe_changes_and_saves_the_rest_as_an_artifact()
    {
        var (run, sink) = TestEngine.Convert(TestPaths.Fixture("delta"));
        var result = run.Objects.Single(o => o.Source.ModificationTarget!.Contains("Page 50001"));
        var al = result.AlText!;

        Assert.Contains("pageextension", al);
        Assert.Contains("addafter(\"Salesperson Code\")", al);
        Assert.Contains("addafter(CalculateBalance)", al);
        Assert.Contains("modify(Balance)", al);
        Assert.Contains("trigger OnOpenPage()", al);
        Assert.Contains("SetLoyaltyVisibility;", al);

        var codes = result.Result.Diagnostics.Select(d => d.Code).ToList();
        Assert.Equal(2, codes.Count(c => c == DiagnosticDescriptors.DeltaCodeModification.Code)); // EXIT in original; EXIT in prepended code
        Assert.Equal(2, codes.Count(c => c == DiagnosticDescriptors.DeltaUnsupportedChange.Code)); // SourceExpr change; Deletion
        Assert.Equal(ObjectConversionStatus.RequiresManualWork, result.Result.Status);

        var artifactPath = Assert.Single(result.Result.OutputFiles, f => f.StartsWith(".cal2al/artifacts/", StringComparison.Ordinal));
        var artifact = sink.GetText(artifactPath)!;
        Assert.Contains("OnQueryClosePage", artifact);
        Assert.Contains("Deletion", artifact);
        Assert.Contains("Ship-to Code 2", artifact);
    }

    [Fact]
    public void Delta_of_a_codeunit_produces_no_object_and_keeps_every_change()
    {
        var (run, sink) = TestEngine.Convert(TestPaths.Fixture("delta"));
        var result = run.Objects.Single(o => o.Source.ModificationTarget!.Contains("Codeunit 50010"));

        Assert.Null(result.AlObject);
        Assert.Equal(ObjectConversionStatus.Failed, result.Result.Status);
        Assert.Contains(result.Result.Diagnostics, d => d.Code == DiagnosticDescriptors.DeltaTargetNotSupported.Code);
        var artifact = sink.GetText(Assert.Single(result.Result.OutputFiles))!;
        Assert.Contains("MESSAGE('Posted');", artifact);
    }
}
