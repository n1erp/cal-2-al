using System.Globalization;
using System.Text.RegularExpressions;
using Cal2AL.Core.Model;
using Cal2AL.Parser.Code;

namespace Cal2AL.Converter.Engine;

/// <summary>
/// C/SIDE writes date constants in the regional order of the machine that exported the objects: a US export has
/// <c>12319999D</c>, a French or German one <c>31129999D</c>. The order is detected per source file from constants
/// that are only valid one way, then from the OBJECT-PROPERTIES <c>Date=</c> values, else month-first is assumed.
/// </summary>
public static partial class DateOrderDetector
{
    public sealed record Result(CalDateOrder Order, string Evidence, bool Conflicting);

    public static Result Detect(IEnumerable<CalObject> objects)
    {
        var list = objects.ToList();
        int monthFirstOnly = 0, dayFirstOnly = 0;
        foreach (var body in list.SelectMany(Bodies))
        {
            foreach (var token in CodeLexer.Tokenize(body.Text).Where(t => t.Kind == CodeTokenKind.Date))
            {
                var mdy = DateLiteral.TryParseCal(token.Text, CalDateOrder.MonthDayYear, out var a) && !a.IsBlank;
                var dmy = DateLiteral.TryParseCal(token.Text, CalDateOrder.DayMonthYear, out var b) && !b.IsBlank;
                if (mdy && !dmy)
                {
                    monthFirstOnly++;
                }
                else if (dmy && !mdy)
                {
                    dayFirstOnly++;
                }
            }
        }

        if (monthFirstOnly > 0 || dayFirstOnly > 0)
        {
            var order = dayFirstOnly > monthFirstOnly ? CalDateOrder.DayMonthYear : CalDateOrder.MonthDayYear;
            return new Result(order,
                $"{dayFirstOnly} date constant(s) are only valid day-first and {monthFirstOnly} only month-first",
                monthFirstOnly > 0 && dayFirstOnly > 0);
        }

        int dayFirstDates = 0, monthFirstDates = 0;
        foreach (var obj in list)
        {
            if (obj.ObjectProperties.TryGetValue("Date", out var date) && ObjectDateRegex().Match(date) is { Success: true } m)
            {
                var first = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                var second = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                if (first > 12 && second <= 12)
                {
                    dayFirstDates++;
                }
                else if (second > 12 && first <= 12)
                {
                    monthFirstDates++;
                }
            }
        }

        if (dayFirstDates > 0 || monthFirstDates > 0)
        {
            var order = dayFirstDates > monthFirstDates ? CalDateOrder.DayMonthYear : CalDateOrder.MonthDayYear;
            return new Result(order,
                $"no unambiguous date constant; {dayFirstDates} object date(s) are day-first and {monthFirstDates} month-first",
                dayFirstDates > 0 && monthFirstDates > 0);
        }

        return new Result(CalDateOrder.MonthDayYear, "no evidence; month-first (the documented C/AL format) was assumed", false);
    }

    private static IEnumerable<CalCodeBody> Bodies(CalObject obj)
    {
        foreach (var procedure in obj.Code.Procedures)
        {
            yield return procedure.Body;
        }

        foreach (var node in Walk(obj))
        {
            foreach (var trigger in node.Triggers)
            {
                yield return trigger.Body;
            }

            foreach (var procedure in node.Sections.SelectMany(s => s.Procedures))
            {
                yield return procedure.Body;
            }
        }
    }

    private static IEnumerable<CalNode> Walk(CalNode node) =>
        new[] { node }.Concat(node.Sections.SelectMany(s => s.Items).SelectMany(Walk));

    [GeneratedRegex(@"^\s*(\d{1,2})[./-](\d{1,2})[./-]\d{2,4}\s*$")]
    private static partial Regex ObjectDateRegex();
}
