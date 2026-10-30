using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class NavCloseConnectionException : Exception
{
	public NavCloseConnectionException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavCloseConnectionException()
	{
	}

	private NavCloseConnectionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavCloseConnectionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
