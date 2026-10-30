using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime.Utility;

public static class TimeSpanExtensions
{
	public static string ToStringWithMilliseconds(this TimeSpan timeSpan)
	{
		return timeSpan.ToString("hh\\:mm\\:ss\\.fff", CultureInfo.InvariantCulture);
	}
}
