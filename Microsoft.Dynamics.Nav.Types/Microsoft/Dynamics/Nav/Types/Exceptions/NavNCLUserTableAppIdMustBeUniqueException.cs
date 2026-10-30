using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLUserTableAppIdMustBeUniqueException : NavNCLTableConstraintBaseException
{
	public NavNCLUserTableAppIdMustBeUniqueException()
	{
	}

	public NavNCLUserTableAppIdMustBeUniqueException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLUserTableAppIdMustBeUniqueException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLUserTableAppIdMustBeUniqueException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
