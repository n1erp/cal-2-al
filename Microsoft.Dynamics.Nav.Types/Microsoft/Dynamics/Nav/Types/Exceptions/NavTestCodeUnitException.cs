using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTestCodeUnitException : NavException
{
	public NavTestCodeUnitException()
	{
	}

	public NavTestCodeUnitException(string message)
		: base(message)
	{
	}

	public NavTestCodeUnitException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTestCodeUnitException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
