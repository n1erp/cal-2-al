using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavCallbackCommunicationException : NavBaseException
{
	[Obsolete("Only use the constructor taking an existing exception")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavCallbackCommunicationException()
	{
	}

	[Obsolete("Only use the constructor taking an existing exception")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavCallbackCommunicationException(string message)
		: base(message)
	{
	}

	public NavCallbackCommunicationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[Obsolete("Only use the constructor taking an existing exception")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavCallbackCommunicationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
