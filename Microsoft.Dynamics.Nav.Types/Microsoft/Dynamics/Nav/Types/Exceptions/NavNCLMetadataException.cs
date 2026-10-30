using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public abstract class NavNCLMetadataException : NavException
{
	protected NavNCLMetadataException()
	{
	}

	protected NavNCLMetadataException(string message)
		: base(message)
	{
	}

	protected NavNCLMetadataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLMetadataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
