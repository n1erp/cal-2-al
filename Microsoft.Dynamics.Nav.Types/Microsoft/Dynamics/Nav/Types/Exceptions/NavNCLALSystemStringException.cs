using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLALSystemStringException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLALSystemStringException()
	{
	}

	public NavNCLALSystemStringException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLALSystemStringException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLALSystemStringException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
