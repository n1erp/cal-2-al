using Cal2AL.Converter.Comparison;

namespace Cal2AL.Tests.Comparison;

public class AlComparerTests
{
    private const string Cal2AL = """
        table 50000 "Cal2AL Test Customer"
        {
            Caption = 'Test Customer';

            fields
            {
                field(1; "No."; Code[20])
                {
                    Caption = 'No.';

                    trigger OnValidate()
                    begin
                        IF "No." <> xRec."No." THEN
                          TestNoSeries;
                    end;
                }
            }

            var
                Text000: Label 'You cannot delete.';

            procedure IsInFiscalYear(PostingDate: Date): Boolean
            begin
                EXIT(PostingDate >= 20250101D);
            end;
        }
        """;

    // What a converter that copies date constants and comments out unsupported properties might produce.
    private const string Reference = """
        table 50000 "Cal2AL Test Customer"
        {
            caption = 'Test Customer';

            fields
            {
                field(1;"No.";Code[20])
                {
                    Caption = 'No.';

                    trigger OnValidate()
                    begin
                        if "No." <> xRec."No." then
                          TestNoSeries;
                    end;
                }
            }

            procedure IsInFiscalYear(PostingDate: Date): Boolean
            begin
                EXIT(PostingDate >= 010125D);
            end;
        }
        """;

    private static ObjectComparison CompareSamples()
    {
        var a = Assert.Single(AlOutlineReader.Read(Cal2AL, "a.al"));
        var b = Assert.Single(AlOutlineReader.Read(Reference, "b.al"));
        Assert.Equal(a.Key, b.Key);
        return AlComparer.Compare(a.Key, a, b);
    }

    [Fact]
    public void Reads_object_structure()
    {
        var outline = Assert.Single(AlOutlineReader.Read(Cal2AL, "a.al"));
        Assert.Equal("table 50000", outline.Key);
        var paths = outline.Entries.Select(e => e.Path).ToList();
        Assert.Contains("Caption", paths);
        Assert.Contains("fields/field(1)", paths);
        Assert.Contains("fields/field(1)/Caption", paths);
        Assert.Contains("fields/field(1)/trigger OnValidate", paths);
        Assert.Contains("var Text000", paths);
        Assert.Contains("procedure IsInFiscalYear", paths);
    }

    [Fact]
    public void Classifies_formatting_behavioural_and_missing_differences()
    {
        var result = CompareSamples();
        DifferenceCategory Category(string path) => result.Entries.Single(e => e.Path == path).Category;

        Assert.Equal(DifferenceCategory.Formatting, Category("Caption"));                        // property name case
        Assert.Equal(DifferenceCategory.Equivalent, Category("fields/field(1)"));                // whitespace only
        Assert.Equal(DifferenceCategory.Equivalent, Category("fields/field(1)/Caption"));
        Assert.Equal(DifferenceCategory.Formatting, Category("fields/field(1)/trigger OnValidate")); // keyword case
        Assert.Equal(DifferenceCategory.Behavioral, Category("procedure IsInFiscalYear"));       // date constant differs
        Assert.Equal(DifferenceCategory.MissingInReference, Category("var Text000"));
    }

    [Fact]
    public void Compares_folders_and_reports_objects_missing_on_either_side()
    {
        var root = Path.Combine(Path.GetTempPath(), "cal2al-compare-" + Guid.NewGuid().ToString("N"));
        var mine = Directory.CreateDirectory(Path.Combine(root, "mine")).FullName;
        var reference = Directory.CreateDirectory(Path.Combine(root, "reference")).FullName;
        try
        {
            File.WriteAllText(Path.Combine(mine, "Customer.Table.al"), Cal2AL);
            File.WriteAllText(Path.Combine(reference, "Tab50000.al"), Reference);
            File.WriteAllText(Path.Combine(reference, "Cod50010.al"), "codeunit 50010 X\n{\n}\n");

            var report = AlComparer.CompareFolders(mine, reference);

            Assert.Equal(["codeunit 50010", "table 50000"], report.Objects.Select(o => o.Key));
            Assert.Null(report.Objects[0].Cal2ALFile);
            Assert.Equal(1, report.Totals[DifferenceCategory.Behavioral]);
            Assert.Contains("\"behavioral\"", report.ToJson(), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
