using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public abstract class NavMetadataException : NavBaseException
{
	public NavMetadataException()
	{
	}

	public NavMetadataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavMetadataException(string message, Exception inner)
		: base(message, inner)
	{
	}

	public NavMetadataException(string message)
		: base(message)
	{
	}
}
