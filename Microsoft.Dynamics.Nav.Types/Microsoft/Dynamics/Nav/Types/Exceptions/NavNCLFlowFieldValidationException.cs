using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLFlowFieldValidationException : NavNCLException
{
	public NavNCLFlowFieldValidationException()
	{
	}

	public NavNCLFlowFieldValidationException(string message)
		: base(message)
	{
	}

	public NavNCLFlowFieldValidationException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected NavNCLFlowFieldValidationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
