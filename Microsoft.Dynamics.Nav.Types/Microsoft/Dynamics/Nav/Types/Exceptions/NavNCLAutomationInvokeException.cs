using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLAutomationInvokeException : NavNCLException
{
	public NavNCLAutomationInvokeException()
	{
	}

	public NavNCLAutomationInvokeException(string message)
		: base(message)
	{
	}

	public NavNCLAutomationInvokeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLAutomationInvokeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLAutomationInvokeException Create(CultureInfo culture, string methodName, Exception innerException)
	{
		string arg = string.Empty;
		if (innerException != null)
		{
			arg = innerException.Message;
		}
		return new NavNCLAutomationInvokeException(string.Format(culture, Lang.AutomationInvokeError, methodName, arg), innerException);
	}

	public static NavNCLAutomationInvokeException CreateNoLanguageSupport(CultureInfo culture, string language)
	{
		return new NavNCLAutomationInvokeException(string.Format(culture, Lang._00095_50169, language));
	}
}
