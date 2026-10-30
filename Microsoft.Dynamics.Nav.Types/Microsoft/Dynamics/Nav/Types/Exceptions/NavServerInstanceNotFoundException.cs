using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavServerInstanceNotFoundException : NavBaseException
{
	public NavServerInstanceNotFoundException()
	{
	}

	public NavServerInstanceNotFoundException(string message)
		: base(message)
	{
	}

	public NavServerInstanceNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavServerInstanceNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
