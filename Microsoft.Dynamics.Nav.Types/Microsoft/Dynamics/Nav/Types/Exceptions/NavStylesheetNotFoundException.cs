using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavStylesheetNotFoundException : NavBaseException
{
	public NavStylesheetNotFoundException()
	{
	}

	public NavStylesheetNotFoundException(string message)
		: base(message)
	{
	}

	public NavStylesheetNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavStylesheetNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
