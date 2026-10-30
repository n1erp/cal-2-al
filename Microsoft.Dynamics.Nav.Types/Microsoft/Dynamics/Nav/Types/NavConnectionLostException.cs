using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class NavConnectionLostException : Exception
{
	protected NavConnectionLostException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavConnectionLostException()
	{
	}

	public NavConnectionLostException(string message)
		: base(message)
	{
	}

	public NavConnectionLostException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
