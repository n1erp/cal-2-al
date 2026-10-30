using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLTypeMappingException : NavNCLException
{
	public NavNCLTypeMappingException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLTypeMappingException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLTypeMappingException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLTypeMappingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
