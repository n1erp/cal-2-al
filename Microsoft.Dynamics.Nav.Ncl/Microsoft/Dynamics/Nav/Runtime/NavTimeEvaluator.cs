using System;
using System.Globalization;
using System.Xml;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavTimeEvaluator : NavValueEvaluator<NavTime>
{
	private static readonly NavTimeEvaluator instance = new NavTimeEvaluator();

	internal static NavTimeEvaluator Instance => instance;

	private NavTimeEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavTime value, INavValueMetadata metadata, string source, int number)
	{
		try
		{
			if (number == 9)
			{
				return XMLFormatEvaluate(out value, source);
			}
			return NonXMLFormatEvaluate(out value, source);
		}
		catch (NavBaseException) when (dataError == DataError.TrapError)
		{
			value = NavTime.Default;
			return false;
		}
	}

	private static bool NonXMLFormatEvaluate(out NavTime value, string source)
	{
		value = NavTime.Default;
		if (string.IsNullOrWhiteSpace(source))
		{
			return true;
		}
		if (DateTimeParsingHelper.TryParseExactTime(source, NavCurrentSessionWrapper.RegionalSettings, out var result) || DateTimeParsingHelper.TryParseExactTime(source, out result))
		{
			value = CreateTime(result.Hour, result.Minute, result.Second, result.Millisecond, source);
			return true;
		}
		int position = 0;
		NavValueEvaluator.ConsumeWhitespace(source, ref position);
		if (CurrentTimeEvaluate(out value, source, position))
		{
			return true;
		}
		int[] array = NavValueEvaluator.EvaluateNumberSequence(source, ref position, 4, 3, 3, out var numberOfDigitsInLongNumber);
		if (array == null || array[0] < 0)
		{
			return false;
		}
		array[1] = Math.Max(array[1], 0);
		array[2] = Math.Max(array[2], 0);
		array[3] = Math.Max(array[3], 0);
		if (numberOfDigitsInLongNumber > 0)
		{
			int[] array2 = new int[3] { 1, 10, 100 };
			array[3] *= array2[3 - numberOfDigitsInLongNumber];
		}
		NavValueEvaluator.ConsumeWhitespace(source, ref position);
		string timeAMDesignator = NavCurrentSessionWrapper.FormatSettings.TimeAMDesignator;
		string timePMDesignator = NavCurrentSessionWrapper.FormatSettings.TimePMDesignator;
		if (position != source.Length)
		{
			if (string.IsNullOrEmpty(timeAMDesignator) || string.IsNullOrEmpty(timePMDesignator) || array[0] > 12)
			{
				return false;
			}
			string value2 = source.Substring(position).TrimEnd();
			bool flag = NavValueEvaluator.StartsWith(timePMDesignator, 0, value2);
			if (!flag && !NavValueEvaluator.StartsWith(timeAMDesignator, 0, value2))
			{
				return false;
			}
			if (flag)
			{
				if (array[0] != 12)
				{
					array[0] += 12;
				}
			}
			else if (array[0] == 12)
			{
				array[0] = 0;
			}
		}
		value = CreateTime(array[0], array[1], array[2], array[3], source);
		return true;
	}

	private static bool CurrentTimeEvaluate(out NavTime value, string source, int position)
	{
		string time = NavCurrentSessionWrapper.FormatSettings.Time;
		if (NavValueEvaluator.StartsWith(time, 0, source, position))
		{
			value = NavTime.CreateTime(NavDateTime.Now.ClientLocalValue);
			return true;
		}
		value = NavTime.Default;
		return false;
	}

	private static bool XMLFormatEvaluate(out NavTime value, string source)
	{
		value = NavTime.Default;
		string originalString = source;
		source = source.Trim();
		if (string.IsNullOrWhiteSpace(source))
		{
			return true;
		}
		if (source.Length > 2 && source[2] != ':')
		{
			return false;
		}
		try
		{
			string s = "0001-01-02T" + source;
			DateTime dateTime = XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.Local);
			value = CreateTime(dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond, originalString);
			return true;
		}
		catch (FormatException)
		{
			return false;
		}
	}

	private static NavTime CreateTime(int hours, int minutes, int seconds, int milliseconds, string originalString)
	{
		if (hours < 0 || hours >= 24 || minutes < 0 || minutes >= 60 || seconds < 0 || seconds >= 60 || milliseconds < 0)
		{
			throw ThrowNavTimeEvalError(originalString);
		}
		try
		{
			return NavTime.Create(hours, minutes, seconds, milliseconds);
		}
		catch (NavNCLOutsidePermittedRangeException)
		{
			throw ThrowNavTimeEvalError(originalString);
		}
	}

	private static NavNCLEvaluateException ThrowNavTimeEvalError(string originalString)
	{
		throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.Time, originalString);
	}

	protected override NavTime GetDefaultValue(INavValueMetadata metadata)
	{
		return NavTime.Default;
	}
}
