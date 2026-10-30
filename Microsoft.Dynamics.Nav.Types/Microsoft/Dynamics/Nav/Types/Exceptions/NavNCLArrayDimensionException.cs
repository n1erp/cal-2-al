using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLArrayDimensionException : NavNCLException
{
	public NavNCLArrayDimensionException()
	{
	}

	public NavNCLArrayDimensionException(string message)
		: base(message)
	{
	}

	public NavNCLArrayDimensionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLArrayDimensionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
