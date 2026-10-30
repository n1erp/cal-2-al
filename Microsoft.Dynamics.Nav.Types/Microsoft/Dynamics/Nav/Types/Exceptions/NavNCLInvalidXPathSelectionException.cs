using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidXPathSelectionException : NavNCLException
{
	public NavNCLInvalidXPathSelectionException()
	{
	}

	public NavNCLInvalidXPathSelectionException(string message)
		: base(message)
	{
	}

	protected NavNCLInvalidXPathSelectionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavNCLInvalidXPathSelectionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
