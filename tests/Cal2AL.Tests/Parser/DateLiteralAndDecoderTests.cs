using System.Text;
using Cal2AL.Parser.Code;
using Cal2AL.Parser.Txt;

namespace Cal2AL.Tests.Parser;

public class DateLiteralAndDecoderTests
{
    [Theory]
    [InlineData("010125D", CalDateOrder.MonthDayYear, "20250101D")]
    [InlineData("123125D", CalDateOrder.MonthDayYear, "20251231D")]
    [InlineData("12312025D", CalDateOrder.MonthDayYear, "20251231D")]
    [InlineData("063029D", CalDateOrder.MonthDayYear, "20290630D")] // two-digit years below the pivot are 20xx
    [InlineData("063030D", CalDateOrder.MonthDayYear, "19300630D")] // the pivot year and above are 19xx
    [InlineData("311225D", CalDateOrder.DayMonthYear, "20251231D")]
    [InlineData("0D", CalDateOrder.MonthDayYear, "0D")]
    public void Converts_CAL_date_constants_to_AL(string cal, CalDateOrder order, string expected)
    {
        Assert.True(DateLiteral.TryParseCal(cal, order, out var value));
        Assert.Equal(expected, DateLiteral.ToAl(value));
    }

    [Theory]
    [InlineData("133125D")] // month 13
    [InlineData("023025D")] // 30 February
    [InlineData("1225D")]   // wrong length
    public void Rejects_invalid_CAL_dates(string cal) =>
        Assert.False(DateLiteral.TryParseCal(cal, CalDateOrder.MonthDayYear, out _));

    [Fact]
    public void Decodes_utf8_with_bom()
    {
        var bytes = new UTF8Encoding(true).GetPreamble().Concat(Encoding.UTF8.GetBytes("Größe")).ToArray();
        var decoded = TextDecoder.Decode(bytes, TextDecoder.ResolveEncoding("850"));
        Assert.Equal("Größe", decoded.Text);
        Assert.False(decoded.UsedFallback);
    }

    [Fact]
    public void Falls_back_to_the_OEM_code_page_for_non_utf8_bytes()
    {
        var cp850 = TextDecoder.ResolveEncoding("850");
        var decoded = TextDecoder.Decode(cp850.GetBytes("Maße\r\n"), cp850);
        Assert.Equal("Maße\n", decoded.Text);
        Assert.True(decoded.UsedFallback);
    }
}
