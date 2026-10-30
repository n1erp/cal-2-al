using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDateEvaluator : NavValueEvaluator<NavDate>
{
	private const int MaxDigitsInYearNumber = 4;

	private const int MinDigitsInYearNumber = 2;

	private static readonly NavDateEvaluator instance = new NavDateEvaluator();

	internal static NavDateEvaluator Instance => instance;

	private NavDateEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavDate value, INavValueMetadata metadata, string source, int number)
	{
		if (string.IsNullOrWhiteSpace(source))
		{
			value = NavDate.Default;
			return true;
		}
		if (number == 9 || Regex.IsMatch(source, "^\\d{4}-\\d{2}-\\d{2}$"))
		{
			return XMLFormatEvaluate(out value, source);
		}
		string originalString = source;
		source = source.Trim();
		if (source[source.Length - 1] == 'D')
		{
			source = source.Substring(0, source.Length - 1);
		}
		if (DateTimeParsingHelper.TryParseExactDate(source, NavCurrentSessionWrapper.RegionalSettings, out var result) || DateTimeParsingHelper.TryParseExactDate(source, out result))
		{
			value = NavDate.Create(DateTime.SpecifyKind(result, DateTimeKind.Local));
			return true;
		}
		int position = 0;
		bool isClosingDate = ClosingDateEvaluate(source, ref position);
		if (WorkDateTodayEvaluate(out value, source, position, isClosingDate))
		{
			return true;
		}
		if (DayWeekYearEvaluate(out value, source, position, isClosingDate, originalString))
		{
			return true;
		}
		return DayMonthYearEvaluate(out value, source, position, isClosingDate, originalString);
	}

	protected override NavDate GetDefaultValue(INavValueMetadata metadata)
	{
		return NavDate.Default;
	}

	private static bool XMLFormatEvaluate(out NavDate value, string source)
	{
		value = NavDate.Default;
		string originalString = source;
		source = source.Trim();
		if (string.IsNullOrWhiteSpace(source))
		{
			return true;
		}
		if (source.Length > 2 && source[source.Length - 3] != '-')
		{
			return false;
		}
		try
		{
			DateTime dateTime = XmlConvert.ToDateTime(source, XmlDateTimeSerializationMode.Local);
			value = CreateDateFromDMY(dateTime.Day, dateTime.Month, dateTime.Year, isClosingDate: false, originalString);
			return true;
		}
		catch (FormatException)
		{
			return false;
		}
	}

	private static bool ClosingDateEvaluate(string source, ref int position)
	{
		bool result = false;
		string closingDateCharacter = NavCurrentSessionWrapper.FormatSettings.ClosingDateCharacter;
		if (NavValueEvaluator.StartsWith(source, position, closingDateCharacter))
		{
			position += closingDateCharacter.Length;
			if (source.Length == closingDateCharacter.Length || IsWhiteSpaceOrDigit(source, position))
			{
				result = true;
				ConsumeSpecialCharactersAndWhitespace(source, ref position);
			}
			else
			{
				position -= closingDateCharacter.Length;
			}
		}
		return result;
	}

	private static bool WorkDateTodayEvaluate(out NavDate value, string source, int position, bool isClosingDate)
	{
		string workDate = NavCurrentSessionWrapper.FormatSettings.WorkDate;
		if (NavValueEvaluator.StartsWith(workDate, 0, source, position))
		{
			value = ALSystemDate.ALWorkDate();
			if (isClosingDate)
			{
				value = ALSystemDate.ALClosingDate(value);
			}
			return true;
		}
		string toDay = NavCurrentSessionWrapper.FormatSettings.ToDay;
		if (NavValueEvaluator.StartsWith(toDay, 0, source, position))
		{
			value = ALSystemDate.ALToday;
			if (isClosingDate)
			{
				value = ALSystemDate.ALClosingDate(value);
			}
			return true;
		}
		value = NavDate.Default;
		return false;
	}

	private static bool DayWeekYearEvaluate(out NavDate value, string source, int position, bool isClosingDate, string originalString)
	{
		if (TryReadLetterToken(source, ref position, out var result))
		{
			string[] dateTextWeekdayNames = NavCurrentSessionWrapper.FormatSettings.DateTextWeekdayNames;
			int firstDayOfWeek = (int)NavCurrentSessionWrapper.FormatSettings.ServerCultureInfo.DateTimeFormat.FirstDayOfWeek;
			bool flag = false;
			int num = firstDayOfWeek;
			for (int i = 0; i < dateTextWeekdayNames.Length; i++)
			{
				if (dateTextWeekdayNames[num].StartsWith(result, StringComparison.OrdinalIgnoreCase))
				{
					flag = true;
					break;
				}
				num = (num + 1) % 7;
			}
			if (!flag)
			{
				throw ThrowNavDateEvalError(originalString);
			}
			num = ((num == 0) ? 7 : num);
			ConsumeSpecialCharactersAndWhitespace(source, ref position);
			int[] array = NavValueEvaluator.EvaluateNumberSequence(source, ref position, 2, 1, 4, out var numberOfDigitsInLongNumber);
			if (array == null || position != source.Length)
			{
				throw ThrowNavDateEvalError(originalString);
			}
			NavDate navDate = null;
			if (array[0] < 0)
			{
				navDate = ALSystemDate.ALWorkDate();
				array[0] = ALSystemDate.ALDate2DWY(navDate, 2);
			}
			if (array[1] < 0)
			{
				if (navDate == null)
				{
					navDate = ALSystemDate.ALWorkDate();
				}
				array[1] = ALSystemDate.ALDate2DWY(navDate, 3);
			}
			else if (numberOfDigitsInLongNumber <= 2)
			{
				array[1] = MapShortYearNumberToYear(array[1]);
			}
			try
			{
				value = ALSystemDate.ALDWY2Date(num, array[0], array[1]);
			}
			catch (NavNCLOutsidePermittedRangeException)
			{
				throw ThrowNavDateEvalError(originalString);
			}
			if (isClosingDate)
			{
				value = ALSystemDate.ALClosingDate(value);
			}
			return true;
		}
		value = NavDate.Default;
		return false;
	}

	private static bool DayMonthYearEvaluate(out NavDate value, string source, int position, bool isClosingDate, string originalString)
	{
		value = NavDate.Default;
		if (position >= source.Length)
		{
			return false;
		}
		int[] dayMonthYearIndex = DateTimeHelper.GetDayMonthYearIndex(NavCurrentSessionWrapper.FormatSettings.DateTextEvalOrder);
		int num = dayMonthYearIndex[0];
		int num2 = dayMonthYearIndex[1];
		int num3 = dayMonthYearIndex[2];
		int[] array;
		int numberOfDigitsInLongNumber;
		if (num3 == 0)
		{
			int num4 = position;
			array = NavValueEvaluator.EvaluateNumberSequence(source, ref position, 3, num3, 2, out numberOfDigitsInLongNumber);
			if (array == null || position != source.Length)
			{
				position = num4;
				array = NavValueEvaluator.EvaluateNumberSequence(source, ref position, 3, num3, 4, out numberOfDigitsInLongNumber);
			}
			if (array == null || array[0] < 0)
			{
				return false;
			}
			if (array[0] >= 0 && array[1] >= 0 && array[2] < 0)
			{
				array[2] = array[1];
				array[1] = array[0];
				array[0] = -1;
			}
		}
		else
		{
			array = NavValueEvaluator.EvaluateNumberSequence(source, ref position, 3, num3, 4, out numberOfDigitsInLongNumber);
			if (array == null || array[0] < 0)
			{
				return false;
			}
		}
		if (array[0] >= 0 && array[1] < 0 && array[2] < 0)
		{
			array[num] = array[0];
			array[num2] = -1;
			array[num3] = -1;
		}
		NavDate navDate = null;
		if (array[num] < 0)
		{
			navDate = ALSystemDate.ALWorkDate();
			array[num] = ALSystemDate.ALDate2DMY(navDate, 1);
		}
		if (array[num2] < 0)
		{
			if (navDate == null)
			{
				navDate = ALSystemDate.ALWorkDate();
			}
			array[num2] = ALSystemDate.ALDate2DMY(navDate, 2);
		}
		if (array[num3] < 0)
		{
			if (navDate == null)
			{
				navDate = ALSystemDate.ALWorkDate();
			}
			array[num3] = ALSystemDate.ALDate2DMY(navDate, 3);
		}
		else if (numberOfDigitsInLongNumber <= 2)
		{
			array[num3] = MapShortYearNumberToYear(array[num3]);
		}
		value = CreateDateFromDMY(array[num], array[num2], array[num3], isClosingDate, originalString);
		return true;
	}

	private static bool TryReadLetterToken(string input, ref int position, out string result)
	{
		result = null;
		int num = position;
		while (position < input.Length && char.IsLetter(input, position))
		{
			position++;
		}
		if (num == position)
		{
			return false;
		}
		result = input.Substring(num, position - num);
		return true;
	}

	protected static void ConsumeSpecialCharactersAndWhitespace(string input, ref int position)
	{
		char[] array = new char[4] { '.', ':', '/', '-' };
		while (position < input.Length && (char.IsWhiteSpace(input, position) || Array.IndexOf(array, input[position]) >= 0))
		{
			position++;
		}
	}

	private static bool IsWhiteSpaceOrDigit(string source, int position)
	{
		if (!char.IsWhiteSpace(source, position))
		{
			return char.IsDigit(source, position);
		}
		return true;
	}

	private static NavDate CreateDateFromDMY(int day, int month, int year, bool isClosingDate, string originalString)
	{
		try
		{
			return ALSystemDate.ALDMY2Date(day, month, year, isClosingDate);
		}
		catch (NavNCLOutsidePermittedRangeException)
		{
			throw ThrowNavDateEvalError(originalString);
		}
	}

	private static int MapShortYearNumberToYear(int yearNumber)
	{
		if (yearNumber < 100)
		{
			int dateRangeCentury = NavCurrentSessionWrapper.FormatSettings.DateRangeCentury;
			int num = dateRangeCentury % 100;
			dateRangeCentury -= num;
			yearNumber += dateRangeCentury + ((yearNumber < num) ? 100 : 0);
		}
		return yearNumber;
	}

	private static NavNCLEvaluateException ThrowNavDateEvalError(string originalString)
	{
		throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.Date, originalString);
	}
}
