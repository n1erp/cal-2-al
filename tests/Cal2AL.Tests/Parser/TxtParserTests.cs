using Cal2AL.Core.Model;
using Cal2AL.Parser.Txt;

namespace Cal2AL.Tests.Parser;

public class TxtParserTests
{
    private static TxtFileParseResult Parse(params string[] path) => new TxtFileParser().ParseFile(TestPaths.Fixture(path));

    [Theory]
    [InlineData("classic", "TAB50000.TXT", CalObjectType.Table, 50000)]
    [InlineData("classic", "PAG50001.TXT", CalObjectType.Page, 50001)]
    [InlineData("classic", "PAG50002.TXT", CalObjectType.Page, 50002)]
    [InlineData("classic", "COD50010.TXT", CalObjectType.Codeunit, 50010)]
    [InlineData("classic", "COD50011.TXT", CalObjectType.Codeunit, 50011)]
    [InlineData("classic", "REP50020.TXT", CalObjectType.Report, 50020)]
    [InlineData("classic", "XML50030.TXT", CalObjectType.XmlPort, 50030)]
    [InlineData("classic", "QUE50040.TXT", CalObjectType.Query, 50040)]
    [InlineData("newsyntax", "TAB50050.TXT", CalObjectType.Table, 50050)]
    [InlineData("delta", "TAB50000.DELTA", CalObjectType.Modification, 50000)]
    [InlineData("encoding", "TAB50070.TXT", CalObjectType.Table, 50070)]
    public void Parses_fixture_without_failures(string folder, string file, CalObjectType type, int id)
    {
        var result = Parse(folder, file);

        Assert.Empty(result.Failures);
        var obj = Assert.Single(result.Objects);
        Assert.Equal(type, obj.Type);
        Assert.Equal(id, obj.Id);
        Assert.Null(obj.UnsupportedReason);
    }
}
