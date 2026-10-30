using System;

namespace Microsoft.Dynamics.Nav.Types;

public class DefaultDateTimeNowProvider : DateTimeNowProvider
{
	private static volatile Lazy<DateTimeNowProvider> instance = new Lazy<DateTimeNowProvider>(() => new DefaultDateTimeNowProvider());

	public static DateTimeNowProvider Instance => instance.Value;

	public override DateTime UtcNow => DateTime.UtcNow;

	public override DateTime Now => DateTime.Now;
}
