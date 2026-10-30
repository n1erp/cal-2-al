using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNotificationActionHandlerNotFoundException : NavNotificationException
{
	public static NavNotificationActionHandlerNotFoundException Create(int codeunitId, string functionName)
	{
		return new NavNotificationActionHandlerNotFoundException(string.Format(CultureInfo.CurrentCulture, Lang.NotificationHandlerNotFound, codeunitId, functionName));
	}

	public static NavNotificationActionHandlerNotFoundException Create(int codeunitId, string functionName, Exception innerException)
	{
		return new NavNotificationActionHandlerNotFoundException(string.Format(CultureInfo.CurrentCulture, Lang.NotificationHandlerNotFound, codeunitId, functionName), innerException);
	}

	public NavNotificationActionHandlerNotFoundException()
	{
	}

	public NavNotificationActionHandlerNotFoundException(string message)
		: base(message)
	{
	}

	public NavNotificationActionHandlerNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNotificationActionHandlerNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
