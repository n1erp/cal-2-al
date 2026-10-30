using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLUserTableCannotSetBothAuthEmailAndAppIdException : NavNCLTableConstraintBaseException
{
	public NavNCLUserTableCannotSetBothAuthEmailAndAppIdException()
	{
	}

	public NavNCLUserTableCannotSetBothAuthEmailAndAppIdException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLUserTableCannotSetBothAuthEmailAndAppIdException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLUserTableCannotSetBothAuthEmailAndAppIdException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
