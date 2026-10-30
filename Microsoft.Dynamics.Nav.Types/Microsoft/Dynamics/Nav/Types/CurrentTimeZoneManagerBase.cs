using System;

namespace Microsoft.Dynamics.Nav.Types;

public class CurrentTimeZoneManagerBase
{
	[ThreadStatic]
	private static TimeZoneInfo threadStaticClientTimeZone;

	public static TimeZoneInfo CurrentTimeZone
	{
		get
		{
			if (threadStaticClientTimeZone == null)
			{
				return TimeZoneInfo.Local;
			}
			return threadStaticClientTimeZone;
		}
		set
		{
			threadStaticClientTimeZone = value;
		}
	}
}
