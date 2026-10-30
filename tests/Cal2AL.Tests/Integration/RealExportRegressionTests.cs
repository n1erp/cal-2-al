using System.Text;
using Cal2AL.Converter;
using Cal2AL.Converter.Engine;
using Cal2AL.Converter.Objects;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Parser.Code;
using Cal2AL.Parser.Txt;

namespace Cal2AL.Tests.Integration;

/// <summary>
/// Regressions found by converting complete NAV 2016 (fr-FR) databases (5,028 and 4,676 objects) and comparing the
/// result with Txt2Al's output. Each test reproduces the construct with a minimal object.
/// </summary>
public class RealExportRegressionTests
{
    private static (ConversionRun Run, InMemoryOutputSink Sink) ConvertText(string text, ConversionOptions? options = null)
    {
        var folder = Path.Combine(Path.GetTempPath(), "cal2al-regression-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            File.WriteAllText(Path.Combine(folder, "objects.txt"), text.Replace("\n", "\r\n", StringComparison.Ordinal), new UTF8Encoding(false));
            return TestEngine.Convert(folder, options);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static string Codeunit(string code, string variables = "") => $$"""
        OBJECT Codeunit 50900 Regression
        {
          OBJECT-PROPERTIES
          {
            Date=15/09/15;
            Time=12:00:00;
          }
          PROPERTIES
          {
          }
          CODE
          {
        {{variables}}
            PROCEDURE Run@1();
            BEGIN
        {{code}}
            END;

            BEGIN
            END.
          }
        }

        """;

    [Fact]
    public void Text_constants_whose_strings_start_on_the_next_line_are_parsed()
    {
        var text = Codeunit("      MESSAGE(BodyTxt);", """
                VAR
                  BodyTxt@1012 : TextConst
                    '@@@={Locked}',
                    'ENU="<p>Hello</p>"';

            """);
        var result = new TxtFileParser().ParseText(text, "x.txt");
        Assert.Empty(result.Failures);
        var variable = Assert.Single(result.Objects[0].Code.Variables);
        Assert.Contains("ENU=", variable.Type.TextConstValue);
    }

    [Fact]
    public void Unnamed_return_values_are_parsed()
    {
        var text = Codeunit(string.Empty).Replace("    PROCEDURE Run@1();", "    LOCAL PROCEDURE CreateText@1(No@1001 : Code[20]) @1000 : Text[80];", StringComparison.Ordinal);
        var result = new TxtFileParser().ParseText(text, "x.txt");
        Assert.Empty(result.Failures);
        var procedure = Assert.Single(result.Objects[0].Code.Procedures);
        Assert.Equal(string.Empty, procedure.ReturnValue!.Name);
        Assert.Equal("Text", procedure.ReturnValue.Type.Name, ignoreCase: true);
    }

    [Fact]
    public void Set_elements_can_be_comparisons_and_boolean_expressions()
    {
        var body = CodeParser.ParseBody("IF TRUE IN [A <> B, NOT (C OR D), X = 0DT] THEN\n  EXIT;");
        Assert.Single(body.Statements);
    }

    [Fact]
    public void Day_first_date_constants_are_detected_and_converted()
    {
        var (run, sink) = ConvertText(Codeunit("      IF WORKDATE > 31129999D THEN\n        EXIT;\n      D := 010125D;", "    VAR\n      D@1000 : Date;\n"));
        var al = sink.GetText(run.Report.GeneratedFiles.Single(f => f.EndsWith(".al", StringComparison.Ordinal)))!;
        Assert.Contains("99991231D", al);
        Assert.Contains("20250101D", al);
        Assert.Contains(run.Report.RunDiagnostics, d => d.Code == DiagnosticDescriptors.DateOrderDetected.Code && d.Message.Contains("DayMonthYear", StringComparison.Ordinal));
        Assert.DoesNotContain(run.Report.AllDiagnostics, d => d.Code == DiagnosticDescriptors.DateConstant.Code && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Date_order_falls_back_to_object_dates_then_month_first()
    {
        var parser = new TxtFileParser();
        var dayFirst = parser.ParseText(Codeunit("      EXIT;"), "a.txt").Objects;
        Assert.Equal(CalDateOrder.DayMonthYear, DateOrderDetector.Detect(dayFirst).Order); // Date=15/09/15

        var unknown = parser.ParseText(Codeunit("      EXIT;").Replace("Date=15/09/15;", "Date=01/02/15;", StringComparison.Ordinal), "b.txt").Objects;
        Assert.Equal(CalDateOrder.MonthDayYear, DateOrderDetector.Detect(unknown).Order);
    }

    [Fact]
    public void Word_layouts_encoded_in_padded_chunks_are_decoded()
    {
        var bytes = Enumerable.Range(0, 100).Select(i => (byte)i).ToArray();
        var lines = new[] { Convert.ToBase64String(bytes[..40]), Convert.ToBase64String(bytes[40..]) }; // first chunk ends with padding
        Assert.EndsWith("=", lines[0]);
        Assert.Equal(bytes, ReportConverter.DecodeLayout(lines));
    }

    [Fact]
    public void Query_count_columns_have_no_data_source()
    {
        var (run, sink) = ConvertText("""
            OBJECT Query 50901 Counting
            {
              OBJECT-PROPERTIES
              {
              }
              PROPERTIES
              {
              }
              ELEMENTS
              {
                { 1   ;    ;DataItem;Customer            ;
                           DataItemTable=Table18 }

                { 2   ;1   ;Column  ;NoOfCustomers       ;
                           MethodType=Totals;
                           Method=Count }

              }
              CODE
              {

                BEGIN
                END.
              }
            }

            """);
        var al = sink.GetText(run.Report.GeneratedFiles.Single(f => f.EndsWith(".al", StringComparison.Ordinal)))!;
        Assert.Contains("column(NoOfCustomers)", al);
        Assert.Contains("Method = Count;", al);
        Assert.DoesNotContain(run.Report.AllDiagnostics, d => d.Code == DiagnosticDescriptors.MissingRequiredPart.Code);
    }

    private const string TableWithOrdinalsAndPlaceholderKey = """
        OBJECT Table 50902 CRM Like
        {
          OBJECT-PROPERTIES
          {
          }
          PROPERTIES
          {
          }
          FIELDS
          {
            { 1   ;   ;User ID             ;Code50        }
            { 2   ;   ;Status              ;Option        ;OptionOrdinalValues=[0;1;2;3];
                                                           OptionString=Empty,Approved,Pending,Rejected }
          }
          KEYS
          {
            {    ;User ID                                 ;Clustered=Yes }
            { No ;                                         }
          }
          CODE
          {

            BEGIN
            END.
          }
        }

        """;

    [Fact]
    public void Option_ordinal_value_lists_use_commas()
    {
        var (run, sink) = ConvertText(TableWithOrdinalsAndPlaceholderKey);
        var al = sink.GetText(run.Report.GeneratedFiles.Single(f => f.EndsWith(".al", StringComparison.Ordinal)))!;
        Assert.Contains("OptionOrdinalValues = 0, 1, 2, 3;", al);
    }

    [Fact]
    public void Disabled_placeholder_keys_without_fields_are_not_emitted()
    {
        var (run, sink) = ConvertText(TableWithOrdinalsAndPlaceholderKey);
        var al = sink.GetText(run.Report.GeneratedFiles.Single(f => f.EndsWith(".al", StringComparison.Ordinal)))!;
        Assert.Contains("key(Key1; \"User ID\")", al);
        Assert.DoesNotContain("; )", al);
    }

    [Fact]
    public void Provider_id_is_resolved_to_the_providing_part_name()
    {
        var (run, sink) = ConvertText("""
            OBJECT Page 50903 Provider Test
            {
              OBJECT-PROPERTIES
              {
              }
              PROPERTIES
              {
                SourceTable=Table18;
              }
              CONTROLS
              {
                { 1   ;0   ;Container ;
                            ContainerType=ContentArea }

                { 10  ;1   ;Part      ;
                            Name=Lines;
                            PagePartID=Page21 }

                { 20  ;1   ;Part      ;
                            PagePartID=Page22;
                            ProviderID=10 }

              }
              CODE
              {

                BEGIN
                END.
              }
            }

            """);
        var al = sink.GetText(run.Report.GeneratedFiles.Single(f => f.EndsWith(".al", StringComparison.Ordinal)))!;
        Assert.Contains("Provider = Lines;", al);
    }

    [Fact]
    public void Framework_assembly_versions_are_unified_into_one_declaration()
    {
        var collector = new DotNetCollector();
        var a = collector.GetAlias("'mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'.System.Globalization.CultureInfo");
        var b = collector.GetAlias("'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'.System.Globalization.CultureInfo");
        Assert.Equal(a, b);
        var type = Assert.Single(collector.Types);
        Assert.Equal("4.0.0.0", type.Version);
    }
}
