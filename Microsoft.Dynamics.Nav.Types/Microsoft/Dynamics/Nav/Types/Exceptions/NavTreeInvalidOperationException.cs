using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTreeInvalidOperationException : NavException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavTreeInvalidOperationException()
	{
	}

	public NavTreeInvalidOperationException(string message)
		: base(message)
	{
	}

	public NavTreeInvalidOperationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTreeInvalidOperationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
