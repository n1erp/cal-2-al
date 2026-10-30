using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLEnumerationNotMappedException : NavNCLException
{
	public NavNCLEnumerationNotMappedException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLEnumerationNotMappedException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLEnumerationNotMappedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLEnumerationNotMappedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
