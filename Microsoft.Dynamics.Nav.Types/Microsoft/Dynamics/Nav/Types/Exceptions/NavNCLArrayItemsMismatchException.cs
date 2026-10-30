using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLArrayItemsMismatchException : NavNCLException
{
	public NavNCLArrayItemsMismatchException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLArrayItemsMismatchException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLArrayItemsMismatchException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLArrayItemsMismatchException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
