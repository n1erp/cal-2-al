using System;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDateTimeEvaluator : NavValueEvaluator<NavDateTime>
{
	private static readonly NavDateTimeEvaluator instance = new NavDateTimeEvaluator();

	internal static NavDateTimeEvaluator Instance => instance;

	private NavDateTimeEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavDateTime value, INavValueMetadata metadata, string source, int number)
	{
		if (number == 9)
		{
			return XMLFormatEvaluate(out value, source);
		}
		return NonXMLFormatEvaluate(out value, source, number);
	}

	private static bool NonXMLFormatEvaluate(out NavDateTime value, string source, int number)
	{
		value = NavDateTime.Default;
		if (string.IsNullOrWhiteSpace(source))
		{
			return true;
		}
		source = source.Trim();
		if (DateTimeParsingHelper.TryParseExactDateTime(source, NavCurrentSessionWrapper.RegionalSettings, out var result) || DateTimeParsingHelper.TryParseExactDateTime(source, out result))
		{
			result = DateTime.SpecifyKind(result, DateTimeKind.Local);
		}
		else
		{
			int num = FindDateEnd(source);
			NavDate value2 = null;
			if (!ALSystemVariable.ALEvaluate(DataError.TrapError, ref value2, source.Substring(0, num), number))
			{
				return false;
			}
			NavTime value3 = null;
			if (num == source.Length)
			{
				value3 = NavTime.Default;
			}
			else if (!ALSystemVariable.ALEvaluate(DataError.TrapError, ref value3, source.Substring(num, source.Length - num), number))
			{
				return false;
			}
			result = new DateTime(value2.value.Year, value2.value.Month, value2.value.Day, value3.value.Hour, value3.value.Minute, value3.value.Second, value3.value.Millisecond, DateTimeKind.Local);
		}
		value = NavDateTime.Create(result, DateTimeReferenceFrame.Client);
		return true;
	}

	public static bool HasTimePart(string source)
	{
		string text = source.Trim();
		return FindDateEnd(text, testZuluTimeMarker: true) != text.Length;
	}

	private static int FindDateEnd(string source, bool testZuluTimeMarker = false)
	{
		if (testZuluTimeMarker && source.EndsWith("Z", StringComparison.OrdinalIgnoreCase) && source.Any(char.IsDigit))
		{
			int num = source.IndexOf("T", StringComparison.OrdinalIgnoreCase);
			if (num > -1)
			{
				return num;
			}
		}
		string dateSeparator = NavCurrentSessionWrapper.FormatSettings.DateSeparator;
		int num2 = dateSeparator.IndexOf(' ');
		for (int i = 0; i < source.Length; i++)
		{
			if (source[i] != ' ')
			{
				continue;
			}
			int num3 = i - num2;
			int num4 = num3 + dateSeparator.Length;
			if (num2 == -1 || num3 < 0 || num4 > source.Length || !source.Substring(num3, dateSeparator.Length).Equals(dateSeparator))
			{
				return i;
			}
			if (num4 == source.Length)
			{
				return source.Length;
			}
			if (!char.IsDigit(source[num4]))
			{
				return num4;
			}
			int j;
			for (j = num4; j < source.Length && char.IsDigit(source[j]); j++)
			{
			}
			if (j == source.Length || NavValueEvaluator.StartsWith(dateSeparator, 0, source, j))
			{
				return source.Length;
			}
			if (source[j] != ' ')
			{
				num3 = j;
				num4 = num3 + dateSeparator.Length;
				if (num4 > source.Length || !source.Substring(num3, dateSeparator.Length).Equals(dateSeparator))
				{
					return i;
				}
			}
		}
		return source.Length;
	}

	private static bool XMLFormatEvaluate(out NavDateTime value, string source)
	{
		value = NavDateTime.Default;
		if (string.IsNullOrWhiteSpace(source))
		{
			return true;
		}
		try
		{
			DateTime initValue = XmlConvert.ToDateTime(source, XmlDateTimeSerializationMode.Utc);
			value = NavDateTime.Create(initValue);
			return true;
		}
		catch (FormatException)
		{
			return false;
		}
	}

	protected override NavDateTime GetDefaultValue(INavValueMetadata metadata)
	{
		return NavDateTime.Default;
	}
}
