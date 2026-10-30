using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public class TimeInterval : IEquatable<TimeInterval>
{
	private readonly TimeSpan interval;

	private readonly bool isMaxValue;

	private readonly int totalSeconds;

	private static readonly TimeSpan maxValue = TimeSpan.MaxValue;

	public static readonly TimeInterval MaxValue = new TimeInterval(maxValue);

	public TimeSpan Value => interval;

	public int TotalSeconds => totalSeconds;

	private TimeInterval()
	{
	}

	public TimeInterval(string value)
		: this(Parse(value, string.Empty).Value)
	{
	}

	public TimeInterval(TimeSpan value)
	{
		if (value < TimeSpan.Zero)
		{
			throw new NavConfigurationException(Lang.NegativeTimeSpan, string.Empty, TimeSpan.Zero);
		}
		interval = value;
		isMaxValue = ((value == maxValue) ? true : false);
		totalSeconds = (int)interval.TotalSeconds;
	}

	public static TimeInterval Parse(string settingValue, string settingName)
	{
		if (string.Compare(settingValue, "MaxValue", ignoreCase: true, CultureInfo.CurrentCulture) == 0)
		{
			if (string.Compare(settingName, "ClientServicesReconnectPeriod", ignoreCase: true, CultureInfo.CurrentCulture) == 0)
			{
				return new TimeInterval(new TimeSpan(0, 0, 2147483644));
			}
			return MaxValue;
		}
		if (!TimeSpan.TryParse(settingValue, out var result))
		{
			throw new NavConfigurationException(Lang.UnableToParseConfigSetting, settingName, typeof(TimeSpan).Name);
		}
		if (result < TimeSpan.Zero)
		{
			throw new NavConfigurationException(Lang.NegativeTimeSpan, settingName, TimeSpan.Zero);
		}
		return new TimeInterval(result);
	}

	public static bool TryParse(string settingValue, out TimeInterval result)
	{
		result = null;
		try
		{
			result = Parse(settingValue, string.Empty);
		}
		catch (NavConfigurationException)
		{
			return false;
		}
		return true;
	}

	public override string ToString()
	{
		if (!isMaxValue)
		{
			return Value.ToString();
		}
		return "MaxValue";
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		TimeInterval timeInterval = obj as TimeInterval;
		if (timeInterval == null)
		{
			return false;
		}
		return Value.Equals(timeInterval.Value);
	}

	public bool Equals(TimeInterval other)
	{
		if (other == null)
		{
			return false;
		}
		return Value.Equals(other.Value);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public static bool operator ==(TimeInterval first, TimeInterval second)
	{
		if ((object)first == second)
		{
			return true;
		}
		if ((object)first == null || (object)second == null)
		{
			return false;
		}
		return first.Value.Equals(second.Value);
	}

	public static bool operator !=(TimeInterval first, TimeInterval second)
	{
		return !(first == second);
	}
}
