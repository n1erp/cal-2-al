using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavInvalidRepeaterException : NavBaseException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavInvalidRepeaterException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavInvalidRepeaterException(string message)
		: base(message)
	{
	}

	public NavInvalidRepeaterException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavInvalidRepeaterException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
