using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class NavClientTerminatingException : Exception
{
	public NavClientTerminatingException(string message)
		: base(message)
	{
	}

	public NavClientTerminatingException()
	{
	}

	private NavClientTerminatingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavClientTerminatingException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
