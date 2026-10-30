using System;

namespace Microsoft.Dynamics.Nav.Types;

public class TimeZoneInfoConstructor
{
	public TimeZoneInfo TimeZoneInfo { get; }

	public TimeZoneInfoConstructor(object timeZoneInfo)
	{
		TimeZoneInfo = (TimeZoneInfo)new TimeZoneInfoConverter().ConvertFrom(timeZoneInfo);
	}
}
