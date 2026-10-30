using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLEventInvocationException : NavNCLException
{
	public NavNCLEventInvocationException()
	{
	}

	public NavNCLEventInvocationException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLEventInvocationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLEventInvocationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
