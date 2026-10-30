using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavDotNetFromAlNotSupportedException : NavAutomationClientSideNotSupportedException
{
	public NavDotNetFromAlNotSupportedException()
		: base(Lang.NavDotNetFromAlNotSupportedException)
	{
	}

	public NavDotNetFromAlNotSupportedException(string message)
		: base(message)
	{
	}

	public NavDotNetFromAlNotSupportedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavDotNetFromAlNotSupportedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
