using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavClientComException : NavClientBaseException
{
	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClientComException()
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClientComException(string message)
		: base(message)
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected NavClientComException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavClientComException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
