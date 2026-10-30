using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavEmptyMediaStreamException : NavMediaBaseException
{
	public NavEmptyMediaStreamException()
		: this(Lang.EmptyMediaStream)
	{
	}

	public NavEmptyMediaStreamException(string message)
		: base(message)
	{
	}

	public NavEmptyMediaStreamException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavEmptyMediaStreamException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
