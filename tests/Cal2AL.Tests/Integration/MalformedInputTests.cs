using Cal2AL.Converter.Engine;
using Cal2AL.Diagnostics;
using Cal2AL.Parser.Txt;

namespace Cal2AL.Tests.Integration;

public class MalformedInputTests
{
    [Fact]
    public void A_broken_object_does_not_lose_the_other_objects_in_the_file()
    {
        var result = new TxtFileParser().ParseFile(TestPaths.Fixture("malformed", "MULTI.TXT"));

        Assert.Equal([50012, 50014], result.Objects.Select(o => o.Id));
        var failure = Assert.Single(result.Failures);
        Assert.Equal(50013, failure.Id);
        Assert.Equal(DiagnosticDescriptors.TxtSyntaxError.Code, failure.Diagnostic.Code);
        Assert.NotNull(failure.Diagnostic.Location.Line);
        Assert.Contains("OBJECT Codeunit 50013", failure.RawText);
    }

    [Fact]
    public void Failed_objects_are_reported_and_saved_as_artifacts()
    {
        var (run, sink) = TestEngine.Convert(TestPaths.Fixture("malformed"));
        var failed = run.Report.Objects.Single(o => o.ObjectId == 50013);
        Assert.Equal(ObjectConversionStatus.Failed, failed.Status);
        Assert.Contains("OBJECT Codeunit 50013", sink.GetText(Assert.Single(failed.OutputFiles)));
    }

    [Fact]
    public void Code_syntax_errors_still_produce_AL_with_the_code_kept()
    {
        var (run, _) = TestEngine.Convert(TestPaths.Fixture("malformed"));
        var obj = run.Objects.Single(o => o.Source.Id == 50014);
        Assert.Equal(ObjectConversionStatus.RequiresManualWork, obj.Result.Status);
        Assert.Contains(obj.Result.Diagnostics, d => d.Code == DiagnosticDescriptors.CodeSyntaxError.Code);
        Assert.NotNull(obj.AlText);
    }

    [Fact]
    public void A_file_without_objects_is_reported()
    {
        var result = new TxtFileParser().ParseFile(TestPaths.Fixture("malformed", "NOTANOBJECT.TXT"));
        Assert.Empty(result.Objects);
        Assert.Contains(result.Diagnostics, d => d.Code == DiagnosticDescriptors.NotAnObjectFile.Code);
    }

    [Fact]
    public void A_missing_source_is_reported_without_throwing()
    {
        var run = Cal2AL.Converter.ServiceCollectionExtensions.CreateDefaultEngine()
            .Run(new ConversionRequest { Source = TestPaths.Fixture("does-not-exist"), Mode = RunMode.Analyze });
        Assert.Contains(run.Report.RunDiagnostics, d => d.Code == DiagnosticDescriptors.FileReadError.Code);
        Assert.Empty(run.Objects);
    }

    [Fact]
    public void Validate_mode_reports_code_syntax_errors_without_converting()
    {
        var run = Cal2AL.Converter.ServiceCollectionExtensions.CreateDefaultEngine()
            .Run(new ConversionRequest { Source = TestPaths.Fixture("malformed"), Mode = RunMode.Validate });
        Assert.All(run.Objects, o => Assert.Null(o.AlText));
        Assert.Contains(run.Report.AllDiagnostics, d => d.Code == DiagnosticDescriptors.CodeSyntaxError.Code);
        Assert.Contains(run.Report.AllDiagnostics, d => d.Code == DiagnosticDescriptors.TxtSyntaxError.Code);
    }

    [Fact]
    public void Truncated_object_is_a_failure_not_a_crash()
    {
        var text = "OBJECT Table 50999 Truncated\n{\n  PROPERTIES\n  {\n    CaptionML=ENU=Truncated;\n";
        var result = new TxtFileParser().ParseText(text, "truncated.txt");
        Assert.Empty(result.Objects);
        Assert.Single(result.Failures);
    }

    [Fact]
    public void Object_filters_skip_objects()
    {
        var (run, sink) = TestEngine.Convert(TestPaths.Fixture("classic"), new Cal2AL.Converter.ConversionOptions
        {
            ObjectTypes = new HashSet<Cal2AL.Core.Model.CalObjectType> { Cal2AL.Core.Model.CalObjectType.Table },
        });
        Assert.All(run.Report.Objects.Where(o => o.ObjectType != "Table"), o => Assert.Equal(ObjectConversionStatus.Skipped, o.Status));
        Assert.Single(sink.Files.Keys, k => k.EndsWith(".al", StringComparison.Ordinal));
    }
}
