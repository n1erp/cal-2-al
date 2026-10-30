using Cal2AL.CLI;

namespace Cal2AL.Tests.Cli;

public class CliOptionsTests
{
    [Fact]
    public void Parses_id_ranges()
    {
        var (ranges, error) = CliOptions.ParseIdRanges("50000..50099, 50200");
        Assert.Null(error);
        Assert.Equal([(50000, 50099), (50200, 50200)], ranges);
    }

    [Theory]
    [InlineData("50099..50000")]
    [InlineData("abc")]
    [InlineData("1..2..3")]
    public void Rejects_invalid_id_ranges(string text) => Assert.NotNull(CliOptions.ParseIdRanges(text).Error);

    [Fact]
    public void Builds_conversion_options_from_the_command_line()
    {
        var options = new CliOptions();
        var command = new System.CommandLine.RootCommand { options.Source, options.ObjectTypes, options.Ids, options.Runtime, options.DateOrder, options.NoTranslations };
        var result = command.Parse(["--source", "x", "--types", "Table,Page", "--ids", "50000..50010", "--runtime", "11.0", "--date-order", "DayMonthYear", "--no-translations"]);
        var (conversion, error) = options.BuildConversionOptions(result);

        Assert.Null(error);
        Assert.Equal(2, conversion!.ObjectTypes.Count);
        Assert.Equal(new Version(11, 0), conversion.TargetRuntime);
        Assert.Equal(Cal2AL.Parser.Code.CalDateOrder.DayMonthYear, conversion.CalDateOrder);
        Assert.False(conversion.GenerateTranslations);
        Assert.True(conversion.VerifyCodePreservation); // option not on this command keeps its default
    }

    [Fact]
    public void Reports_unknown_object_types()
    {
        var options = new CliOptions();
        var command = new System.CommandLine.RootCommand { options.Source, options.ObjectTypes };
        var (_, error) = options.BuildConversionOptions(command.Parse(["--source", "x", "--types", "Dataport2"]));
        Assert.Contains("Dataport2", error);
    }
}
