using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
[EventLog(WriteEntry = false)]
public sealed class NavServerSessionException : NavServerSessionBaseException
{
	[Obsolete("Only use the constructor taking an existing exception")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavServerSessionException()
	{
	}

	[Obsolete("Only use the constructor taking an existing exception")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavServerSessionException(string message)
		: base(message)
	{
	}

	public NavServerSessionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[Obsolete("Only use the constructor taking an existing exception")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavServerSessionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
