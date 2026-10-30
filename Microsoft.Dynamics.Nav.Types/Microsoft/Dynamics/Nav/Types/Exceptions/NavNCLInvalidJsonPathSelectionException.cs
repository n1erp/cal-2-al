using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidJsonPathSelectionException : NavNCLException
{
	public NavNCLInvalidJsonPathSelectionException()
	{
	}

	public NavNCLInvalidJsonPathSelectionException(string message)
		: base(message)
	{
	}

	protected NavNCLInvalidJsonPathSelectionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavNCLInvalidJsonPathSelectionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
