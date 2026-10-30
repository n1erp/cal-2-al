using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLCalculationOverflowException : NavNCLException
{
	public NavNCLCalculationOverflowException()
	{
	}

	public NavNCLCalculationOverflowException(string message)
		: base(message)
	{
	}

	public NavNCLCalculationOverflowException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLCalculationOverflowException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
