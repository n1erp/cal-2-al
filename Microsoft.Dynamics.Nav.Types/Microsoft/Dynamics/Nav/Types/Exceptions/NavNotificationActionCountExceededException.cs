using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNotificationActionCountExceededException : NavNotificationException
{
	public static NavNotificationActionCountExceededException Create(NotificationScope fromScope)
	{
		if (fromScope == NotificationScope.GlobalScope)
		{
			return new NavNotificationActionCountExceededException(Lang.GlobalNotificationActionCountExceeded);
		}
		return new NavNotificationActionCountExceededException(Lang.LocalNotificationActionCountExceeded);
	}

	public NavNotificationActionCountExceededException()
	{
	}

	public NavNotificationActionCountExceededException(string message)
		: base(message)
	{
	}

	public NavNotificationActionCountExceededException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNotificationActionCountExceededException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
