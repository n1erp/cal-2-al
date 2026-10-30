using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLAutomationCreateException : NavNCLException
{
	public NavNCLAutomationCreateException()
	{
	}

	public NavNCLAutomationCreateException(string message)
		: base(message)
	{
	}

	public NavNCLAutomationCreateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLAutomationCreateException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLAutomationCreateException Create(CultureInfo culture, Type objectType, Guid classId, Exception innerException)
	{
		string arg = string.Empty;
		if (innerException != null)
		{
			arg = innerException.Message;
		}
		return new NavNCLAutomationCreateException(string.Format(culture, Lang.AutomationCreateError, objectType, classId, arg), innerException);
	}

	public static NavNCLAutomationCreateException Create(CultureInfo culture, string name, Guid classId, Exception innerException)
	{
		string arg = string.Empty;
		if (innerException != null)
		{
			arg = innerException.Message;
		}
		return new NavNCLAutomationCreateException(string.Format(culture, Lang.AutomationCreateError, name, classId, arg), innerException);
	}

	public static NavNCLAutomationCreateException CreateUnknownServer(CultureInfo culture, Guid classId)
	{
		return new NavNCLAutomationCreateException(string.Format(culture, Lang.AutomationCreateErrorUnknownServer, classId.ToString("B").ToUpperInvariant()));
	}

	public static NavNCLAutomationCreateException CreateComObjectNotAllowed(CultureInfo culture, string className)
	{
		return new NavNCLAutomationCreateException(string.Format(culture, Lang.AutomationComObjectNotAllowed, className));
	}

	public static NavNCLAutomationCreateException CreateComObjectFailedDueToEventHandlerRegistration(CultureInfo culture, string automationName, Guid classId, Exception innerException)
	{
		return new NavNCLAutomationCreateException(string.Format(culture, Lang.AutomationEventRegistrationFailed, automationName, classId), innerException);
	}
}
