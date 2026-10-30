using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLArrayTypeMismatchException : NavNCLException
{
	public NavNCLArrayTypeMismatchException()
	{
	}

	public NavNCLArrayTypeMismatchException(string message)
		: base(message)
	{
	}

	public NavNCLArrayTypeMismatchException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLArrayTypeMismatchException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
