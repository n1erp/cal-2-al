using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
[EventLog(WriteEntry = false)]
public sealed class NavServerSessionNotActiveException : NavServerSessionBaseException
{
	public NavServerSessionNotActiveException()
		: base(Lang.ConnectionLostAndClose)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavServerSessionNotActiveException(string message)
		: base(message)
	{
	}

	[Obsolete("Only use the constructor taking an existing exception")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavServerSessionNotActiveException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[Obsolete("Only use the constructor taking an existing exception")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavServerSessionNotActiveException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
