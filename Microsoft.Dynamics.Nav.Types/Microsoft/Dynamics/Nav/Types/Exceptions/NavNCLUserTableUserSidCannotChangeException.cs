using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLUserTableUserSidCannotChangeException : NavNCLTableConstraintBaseException
{
	public NavNCLUserTableUserSidCannotChangeException()
	{
	}

	public NavNCLUserTableUserSidCannotChangeException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLUserTableUserSidCannotChangeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLUserTableUserSidCannotChangeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLUserTableUserSidCannotChangeException Create()
	{
		return new NavNCLUserTableUserSidCannotChangeException(Lang.UserSidCanNotChange);
	}
}
