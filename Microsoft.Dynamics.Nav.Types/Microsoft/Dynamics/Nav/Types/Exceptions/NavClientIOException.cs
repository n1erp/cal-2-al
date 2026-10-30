using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavClientIOException : NavClientBaseException
{
	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClientIOException()
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClientIOException(string message)
		: base(message)
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected NavClientIOException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavClientIOException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
