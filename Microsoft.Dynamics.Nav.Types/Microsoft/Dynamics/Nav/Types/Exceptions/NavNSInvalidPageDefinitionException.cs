using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNSInvalidPageDefinitionException : NavMetadataException
{
	public NavNSInvalidPageDefinitionException()
	{
	}

	public NavNSInvalidPageDefinitionException(string message)
		: base(message)
	{
	}

	public NavNSInvalidPageDefinitionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNSInvalidPageDefinitionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
