using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavAutomationClientSideNotSupportedException : NavException
{
	public NavAutomationClientSideNotSupportedException()
		: base(Lang.AutomationClientSideNotSupportedForDisplayTarget)
	{
	}

	public NavAutomationClientSideNotSupportedException(string message)
		: base(message)
	{
	}

	public NavAutomationClientSideNotSupportedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavAutomationClientSideNotSupportedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
