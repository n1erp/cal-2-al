using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNotificationException : NavException
{
	public static NavNotificationException Create(string message)
	{
		return new NavNotificationException(message);
	}

	public NavNotificationException()
	{
	}

	public NavNotificationException(string message)
		: base(message)
	{
	}

	public NavNotificationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNotificationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
