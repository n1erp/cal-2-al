using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLAutomationMethodNotFoundException : NavNCLException
{
	public NavNCLAutomationMethodNotFoundException()
	{
	}

	public NavNCLAutomationMethodNotFoundException(string message)
		: base(message)
	{
	}

	public NavNCLAutomationMethodNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLAutomationMethodNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLAutomationMethodNotFoundException CreateForEvent(CultureInfo culture, string methodName, string interfaceName)
	{
		return new NavNCLAutomationMethodNotFoundException(string.Format(culture, Lang._00095_50118, methodName, interfaceName));
	}
}
