using System.Text;
using Cal2AL.Converter.Engine;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;

namespace Cal2AL.Tests.Integration;

/// <summary>
/// Converts the whole fixture set and checks the result against approved output in tests/Expected.
/// Set CAL2AL_UPDATE_SNAPSHOTS=1 to rewrite the approved files after reviewing a deliberate change.
/// </summary>
public class FixtureConversionTests
{
    private static readonly Lazy<(ConversionRun Run, InMemoryOutputSink Sink)> Converted = new(() => TestEngine.Convert(TestPaths.Fixtures));

    private static ConversionRun Run => Converted.Value.Run;
    private static InMemoryOutputSink Sink => Converted.Value.Sink;

    public static TheoryData<string> GeneratedFiles()
    {
        var data = new TheoryData<string>();
        foreach (var path in Converted.Value.Sink.Files.Keys.Where(p => !p.EndsWith("report.json", StringComparison.Ordinal)).Order(StringComparer.Ordinal))
        {
            data.Add(path);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(GeneratedFiles))]
    public void Generated_file_matches_the_approved_output(string relativePath)
    {
        var actual = Normalize(Sink.GetText(relativePath)!);
        var approvedPath = Path.Combine(TestPaths.SourceTestsFolder(), "Expected", relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (Environment.GetEnvironmentVariable("CAL2AL_UPDATE_SNAPSHOTS") == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(approvedPath)!);
            File.WriteAllText(approvedPath, actual, new UTF8Encoding(false));
        }

        Assert.True(File.Exists(approvedPath), $"No approved output for {relativePath}. Review the file and run with CAL2AL_UPDATE_SNAPSHOTS=1.");
        Assert.Equal(Normalize(File.ReadAllText(approvedPath)), actual);
    }

    [Fact]
    public void Every_approved_file_is_still_generated()
    {
        var expectedRoot = Path.Combine(TestPaths.SourceTestsFolder(), "Expected");
        var approved = Directory.EnumerateFiles(expectedRoot, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(expectedRoot, f).Replace(Path.DirectorySeparatorChar, '/'));
        Assert.All(approved, path => Assert.True(Sink.Files.ContainsKey(path), $"{path} is approved but no longer generated."));
    }

    [Fact]
    public void Statements_and_database_operations_are_preserved_in_every_converted_object()
    {
        foreach (var obj in Run.Objects.Where(o => o.AlObject is not null && o.Source.Type != CalObjectType.Modification))
        {
            var m = obj.Result.Metrics;
            Assert.True(m.Statements == m.OutputStatements, $"{obj.Source.DisplayName}: statements {m.Statements} -> {m.OutputStatements}");
            Assert.True(m.DatabaseOperations == m.OutputDatabaseOperations, $"{obj.Source.DisplayName}: DB operations {m.DatabaseOperations} -> {m.OutputDatabaseOperations}");
        }
    }

    [Fact]
    public void No_preservation_check_failures()
    {
        Assert.DoesNotContain(Run.Report.AllDiagnostics, d => d.Code == DiagnosticDescriptors.PreservationCheckFailed.Code);
    }

    [Fact]
    public void Every_diagnostic_has_a_location_and_a_resolution_or_reason()
    {
        foreach (var d in Run.Report.AllDiagnostics)
        {
            Assert.False(string.IsNullOrEmpty(d.Location.File), $"{d.Code} has no file");
            if (d.Severity == DiagnosticSeverity.Error)
            {
                Assert.False(string.IsNullOrEmpty(d.SuggestedResolution) && string.IsNullOrEmpty(d.Reason), $"{d.Code}: {d.Message} has neither reason nor resolution");
            }
        }
    }

    [Theory]
    [InlineData(CalObjectType.Table, 50000, ObjectConversionStatus.Converted)]
    [InlineData(CalObjectType.Page, 50001, ObjectConversionStatus.Converted)]
    [InlineData(CalObjectType.Page, 50002, ObjectConversionStatus.Converted)]
    [InlineData(CalObjectType.Codeunit, 50010, ObjectConversionStatus.ConvertedWithWarnings)] // two-digit-year date constant
    [InlineData(CalObjectType.Codeunit, 50011, ObjectConversionStatus.RequiresManualWork)]
    [InlineData(CalObjectType.Report, 50020, ObjectConversionStatus.Converted)]
    [InlineData(CalObjectType.XmlPort, 50030, ObjectConversionStatus.Converted)]
    [InlineData(CalObjectType.Query, 50040, ObjectConversionStatus.Converted)]
    [InlineData(CalObjectType.Table, 50050, ObjectConversionStatus.Converted)]
    [InlineData(CalObjectType.Form, 50060, ObjectConversionStatus.Failed)]
    [InlineData(CalObjectType.MenuSuite, 1090, ObjectConversionStatus.Failed)]
    public void Object_has_the_expected_status(CalObjectType type, int id, ObjectConversionStatus status)
    {
        var result = Run.Report.Objects.Single(o => o.ObjectType == type.ToString() && o.ObjectId == id);
        Assert.Equal(status, result.Status);
    }

    [Fact]
    public void Unsupported_objects_are_saved_verbatim_as_artifacts()
    {
        var form = Run.Report.Objects.Single(o => o.ObjectType == "Form");
        var artifact = Assert.Single(form.OutputFiles);
        Assert.StartsWith(".cal2al/artifacts/", artifact);
        Assert.Contains("OBJECT Form 50060", Sink.GetText(artifact));
    }

    [Fact]
    public void Report_layout_is_extracted_and_referenced()
    {
        Assert.Contains("layouts/Cal2ALCustomerList.Report.rdlc", Sink.Files.Keys);
        Assert.Contains("RDLCLayout = 'layouts/Cal2ALCustomerList.Report.rdlc';", Sink.GetText("classic/Cal2ALCustomerList.Report.al"));
        Assert.StartsWith("<?xml", Sink.GetText("layouts/Cal2ALCustomerList.Report.rdlc")!.TrimStart());
    }

    [Fact]
    public void Translations_and_dotnet_package_are_generated()
    {
        Assert.Contains("Testdebitor", Sink.GetText("Translations/Cal2AL.de-DE.xlf"));
        Assert.Contains("type('System.Timers.Timer'; Timer)", Sink.GetText("DotNet.Package.al"));
    }

    [Fact]
    public void Special_characters_survive_decoding_and_conversion()
    {
        var al = Sink.GetText("encoding/Cal2ALGrößentabelle.Table.al")!;
        Assert.Contains("table 50070 \"Cal2AL Größentabelle\"", al);
        Assert.Contains("\"Maße 'Außen'\"", al);
        Assert.Contains(Run.Report.AllDiagnostics, d => d.Code == DiagnosticDescriptors.NameChanged.Code);
    }

    [Fact]
    public void Generated_files_start_with_a_utf8_bom()
    {
        var bytes = Sink.Files["classic/Cal2ALTestCustomer.Table.al"];
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes.Take(3));
    }

    [Fact]
    public void Output_is_deterministic()
    {
        var (_, second) = TestEngine.Convert(TestPaths.Fixtures);
        Assert.Equal(Sink.Files.Keys.Order(StringComparer.Ordinal), second.Files.Keys.Order(StringComparer.Ordinal));
        foreach (var key in Sink.Files.Keys.Where(k => !k.EndsWith(".json", StringComparison.Ordinal)))
        {
            Assert.Equal(Sink.GetText(key), second.GetText(key));
        }
    }

    private static string Normalize(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal);
}
