using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNotificationCannotRecallUnknownException : NavNotificationException
{
	public static NavNotificationCannotRecallUnknownException Create()
	{
		return new NavNotificationCannotRecallUnknownException(Lang.NotificationCannotRecallUnknown);
	}

	public NavNotificationCannotRecallUnknownException()
	{
	}

	public NavNotificationCannotRecallUnknownException(string message)
		: base(message)
	{
	}

	public NavNotificationCannotRecallUnknownException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNotificationCannotRecallUnknownException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
