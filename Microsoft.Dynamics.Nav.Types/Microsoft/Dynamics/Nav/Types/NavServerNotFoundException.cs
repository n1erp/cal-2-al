using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class NavServerNotFoundException : NavException
{
	private NavServerNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavServerNotFoundException()
	{
	}

	public NavServerNotFoundException(string message)
		: base(message)
	{
	}

	public NavServerNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
