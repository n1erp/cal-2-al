using Cal2AL.Converter;
using Cal2AL.Converter.Symbols;
using Cal2AL.Converter.Values;
using Cal2AL.Core.Model;
using Cal2AL.Parser.Txt;

namespace Cal2AL.Tests.Converter;

public class ValueConverterTests
{
    private static readonly SymbolTable Symbols = BuildSymbols();

    private static SymbolTable BuildSymbols()
    {
        var parsed = new TxtFileParser().ParseFile(TestPaths.Fixture("classic", "TAB50000.TXT"));
        return SymbolTable.Build(parsed.Objects);
    }

    private static FilterExpressionConverter Filters() => new(Symbols);

    [Theory]
    // C/SIDE writes relations with names; table names with spaces are quoted, field names in conditions are not.
    [InlineData("Salesperson/Purchaser", "\"Salesperson/Purchaser\"")]
    [InlineData("\"Cal2AL Test Customer\".No.", "\"Cal2AL Test Customer\".\"No.\"")]
    [InlineData("\"Cal2AL Test Customer\".\"No.\" WHERE (Blocked=CONST(No))", "\"Cal2AL Test Customer\".\"No.\" where(Blocked = const(false))")]
    [InlineData("IF (Customer Type=CONST(Company)) \"Cal2AL Test Customer\" ELSE Contact", "if (\"Customer Type\" = const(Company)) \"Cal2AL Test Customer\" else Contact")]
    public void Converts_table_relations(string input, string expected) =>
        Assert.Equal(expected, Filters().ConvertTableRelation(input, 50000));

    [Theory]
    [InlineData("Sum(\"Cust. Ledger Entry\".Amount WHERE (Customer No.=FIELD(No.)))", "sum(\"Cust. Ledger Entry\".Amount where(\"Customer No.\" = field(\"No.\")))")]
    [InlineData("Exist(\"Cal2AL Test Customer\" WHERE (No.=FIELD(No.)))", "exist(\"Cal2AL Test Customer\" where(\"No.\" = field(\"No.\")))")]
    public void Converts_calc_formulas(string input, string expected) =>
        Assert.Equal(expected, Filters().ConvertCalcFormula(input, 50000));

    [Fact]
    public void Converts_table_views_with_sorting_order_and_filters()
    {
        var result = Filters().ConvertTableView("SORTING(Name) ORDER(Descending) WHERE(Blocked=CONST(Yes),No.=FILTER(10000..20000))", 50000);
        Assert.Equal("sorting(Name) order(descending) where(Blocked = const(true), \"No.\" = filter(10000..20000))", result);
    }

    [Fact]
    public void Reports_assumed_boolean_constants_for_unknown_tables()
    {
        var filters = Filters();
        var assumed = new List<string>();
        filters.AssumedBoolean += assumed.Add;
        var result = filters.ConvertTableView("WHERE(Open=CONST(Yes))", 21);
        Assert.Equal("where(Open = const(true))", result);
        Assert.Single(assumed);
    }

    [Fact]
    public void Keeps_yes_no_option_values_on_known_non_boolean_fields()
    {
        // Field 3 "Customer Type" of table 50000 is an Option, not a Boolean.
        var result = Filters().ConvertTableView("WHERE(Customer Type=CONST(Company))", 50000);
        Assert.Equal("where(\"Customer Type\" = const(Company))", result);
    }

    [Fact]
    public void Converts_query_order_by() =>
        Assert.Equal("descending(Balance), ascending(Name)", FilterExpressionConverter.ConvertOrderBy("Balance=Descending,Name=Ascending"));

    [Fact]
    public void Parses_multi_language_text_with_escaped_semicolons()
    {
        var entries = MultiLanguageText.Parse("DEU=Nr.;ENU=\"No.; Name\"");
        Assert.NotNull(entries);
        Assert.Equal(2, entries!.Count);
        Assert.Equal("de-DE", MultiLanguageText.ToCultureName("DEU"));
    }

    [Fact]
    public void Converts_captions_to_labels_and_collects_translations()
    {
        var obj = new TxtFileParser().ParseFile(TestPaths.Fixture("classic", "TAB50000.TXT")).Objects[0];
        var run = new RunContext(new ConversionOptions(), Symbols);
        var context = new ObjectContext(run, obj);

        var label = LabelConverter.Convert(context, "@@@=Shown on the card;DEU=Debitor;ENU=Customer", "Table X - Property Caption", 1);

        Assert.Equal("'Customer', Comment = 'Shown on the card'", label);
        var unit = Assert.Single(run.Translations.Units);
        Assert.Equal("Debitor", unit.Translations["de-DE"]);
    }

    [Fact]
    public void Locked_labels_are_not_translated()
    {
        var obj = new TxtFileParser().ParseFile(TestPaths.Fixture("classic", "TAB50000.TXT")).Objects[0];
        var run = new RunContext(new ConversionOptions(), Symbols);
        var label = LabelConverter.Convert(new ObjectContext(run, obj), "@@@={Locked};ENU=CAL2AL", "X", 1);
        Assert.Equal("'CAL2AL', Locked = true", label);
        Assert.Empty(run.Translations.Units);
    }

    [Fact]
    public void Resolves_table_ids_from_names_and_prefixed_numbers()
    {
        Assert.Equal(50000, Symbols.ResolveTableId("Table50000"));
        Assert.Equal(50000, Symbols.ResolveTableId("Cal2AL Test Customer"));
        Assert.Equal(21, Symbols.ResolveTableId("21"));
        Assert.Equal("Boolean", Symbols.FindField(50000, "Blocked")?.DataType);
        Assert.Equal(CalObjectType.Table, CalObjectTypeExtensions.ParseObjectType("TableData"));
    }
}
