using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLUserTableUserNameMustBeUniqueException : NavNCLTableConstraintBaseException
{
	public NavNCLUserTableUserNameMustBeUniqueException()
	{
	}

	public NavNCLUserTableUserNameMustBeUniqueException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLUserTableUserNameMustBeUniqueException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLUserTableUserNameMustBeUniqueException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLUserTableUserNameMustBeUniqueException Create()
	{
		return new NavNCLUserTableUserNameMustBeUniqueException(Lang.UserNameMustBeUnique);
	}
}
