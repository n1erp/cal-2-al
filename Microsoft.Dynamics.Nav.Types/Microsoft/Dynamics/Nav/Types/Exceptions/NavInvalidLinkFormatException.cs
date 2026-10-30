using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavInvalidLinkFormatException : NavException
{
	public NavInvalidLinkFormatException()
	{
	}

	public NavInvalidLinkFormatException(string message)
		: base(message)
	{
	}

	public NavInvalidLinkFormatException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavInvalidLinkFormatException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
