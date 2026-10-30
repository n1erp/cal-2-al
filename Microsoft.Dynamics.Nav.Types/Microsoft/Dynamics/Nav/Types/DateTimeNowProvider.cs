using System;

namespace Microsoft.Dynamics.Nav.Types;

public abstract class DateTimeNowProvider
{
	private static volatile DateTimeNowProvider current = DefaultDateTimeNowProvider.Instance;

	public static DateTimeNowProvider Current
	{
		get
		{
			return current;
		}
		set
		{
			current = value;
		}
	}

	public abstract DateTime UtcNow { get; }

	public abstract DateTime Now { get; }
}
