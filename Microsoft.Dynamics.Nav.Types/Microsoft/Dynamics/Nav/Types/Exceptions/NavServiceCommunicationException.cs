using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavServiceCommunicationException : NavBaseException
{
	public NavServiceCommunicationException()
	{
	}

	public NavServiceCommunicationException(string message)
		: base(message)
	{
	}

	public NavServiceCommunicationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[Obsolete("Only use the constructor taking an existing exception")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavServiceCommunicationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
