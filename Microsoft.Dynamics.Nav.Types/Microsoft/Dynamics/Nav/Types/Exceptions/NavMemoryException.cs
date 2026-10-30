using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavMemoryException : NavBaseException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavMemoryException()
	{
	}

	public NavMemoryException(string message)
		: base(message)
	{
	}

	public NavMemoryException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavMemoryException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
