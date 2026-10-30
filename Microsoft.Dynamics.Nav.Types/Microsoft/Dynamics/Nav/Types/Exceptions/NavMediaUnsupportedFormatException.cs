using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavMediaUnsupportedFormatException : NavMediaBaseException
{
	public NavMediaUnsupportedFormatException()
	{
	}

	public NavMediaUnsupportedFormatException(string message)
		: base(message)
	{
	}

	public NavMediaUnsupportedFormatException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavMediaUnsupportedFormatException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
