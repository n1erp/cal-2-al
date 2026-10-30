using System.Globalization;

namespace Cal2AL.Parser.Code;

/// <summary>Order of the day and month digits in C/AL date constants.</summary>
public enum CalDateOrder
{
    /// <summary>MMDDYY[YY]D — the documented C/AL format.</summary>
    MonthDayYear,

    /// <summary>DDMMYY[YY]D — for code written on machines where C/SIDE interpreted constants day-first.</summary>
    DayMonthYear,
}

/// <summary>Result of reading a date constant.</summary>
public readonly record struct DateLiteralValue(bool IsBlank, DateOnly Date, bool TwoDigitYear);

/// <summary>
/// Date constants differ between the languages: C/AL writes <c>MMDDYY[YY]D</c>, AL writes <c>yyyymmddD</c>.
/// Copying C/AL code unchanged (as Txt2Al does) turns <c>122101D</c> (21 Dec 2001) into an invalid or different
/// AL date, so Cal2AL rewrites every non-blank date constant. Time constants (<c>hhmmss[.xxx]T</c>) and <c>0DT</c>
/// are written the same way in both languages and are not touched.
/// </summary>
public static class DateLiteral
{
    /// <summary>Two-digit years below this value are read as 20YY, others as 19YY (the NAV convention for date entry).</summary>
    public const int CenturyPivot = 30;

    public static bool TryParseCal(string token, CalDateOrder order, out DateLiteralValue value)
    {
        value = default;
        var digits = StripSuffix(token);
        if (digits is null)
        {
            return false;
        }

        if (digits.All(c => c == '0'))
        {
            value = new DateLiteralValue(true, default, false);
            return true;
        }

        if (digits.Length is not (6 or 8))
        {
            return false;
        }

        var first = int.Parse(digits[..2], CultureInfo.InvariantCulture);
        var second = int.Parse(digits[2..4], CultureInfo.InvariantCulture);
        var year = int.Parse(digits[4..], CultureInfo.InvariantCulture);
        var twoDigit = digits.Length == 6;
        if (twoDigit)
        {
            year += year < CenturyPivot ? 2000 : 1900;
        }

        var (month, day) = order == CalDateOrder.MonthDayYear ? (first, second) : (second, first);
        if (!IsValid(year, month, day))
        {
            return false;
        }

        value = new DateLiteralValue(false, new DateOnly(year, month, day), twoDigit);
        return true;
    }

    public static bool TryParseAl(string token, out DateLiteralValue value)
    {
        value = default;
        var digits = StripSuffix(token);
        if (digits is null)
        {
            return false;
        }

        if (digits.All(c => c == '0'))
        {
            value = new DateLiteralValue(true, default, false);
            return true;
        }

        if (digits.Length != 8)
        {
            return false;
        }

        var year = int.Parse(digits[..4], CultureInfo.InvariantCulture);
        var month = int.Parse(digits[4..6], CultureInfo.InvariantCulture);
        var day = int.Parse(digits[6..], CultureInfo.InvariantCulture);
        if (!IsValid(year, month, day))
        {
            return false;
        }

        value = new DateLiteralValue(false, new DateOnly(year, month, day), false);
        return true;
    }

    public static string ToAl(DateLiteralValue value) =>
        value.IsBlank ? "0D" : value.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "D";

    private static string? StripSuffix(string token)
    {
        if (token.Length < 2 || char.ToUpperInvariant(token[^1]) != 'D')
        {
            return null;
        }

        var digits = token[..^1];
        return digits.All(char.IsAsciiDigit) ? digits : null;
    }

    private static bool IsValid(int year, int month, int day) =>
        year is >= 1 and <= 9999 && month is >= 1 and <= 12 && day >= 1 && day <= DateTime.DaysInMonth(year, month);
}
