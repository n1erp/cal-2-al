using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
[EventLog(WriteEntry = false)]
public sealed class NavServerSessionRemovedException : NavServerSessionBaseException
{
	public NavServerSessionRemovedException()
		: base(Lang.ConnectionLostAndClose)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavServerSessionRemovedException(string message)
		: base(message)
	{
	}

	[Obsolete("Only use the constructor taking an existing exception")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavServerSessionRemovedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[Obsolete("Only use the constructor taking an existing exception")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavServerSessionRemovedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
