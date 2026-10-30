using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class NavTimeZoneException : NavBaseException
{
	private NavTimeZoneException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTimeZoneException()
	{
	}

	public NavTimeZoneException(string message)
		: base(message)
	{
	}

	public NavTimeZoneException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
