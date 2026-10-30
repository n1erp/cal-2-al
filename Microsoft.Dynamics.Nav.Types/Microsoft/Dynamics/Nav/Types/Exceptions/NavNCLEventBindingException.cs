using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLEventBindingException : NavNCLException
{
	public NavNCLEventBindingException()
	{
	}

	public NavNCLEventBindingException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLEventBindingException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLEventBindingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
