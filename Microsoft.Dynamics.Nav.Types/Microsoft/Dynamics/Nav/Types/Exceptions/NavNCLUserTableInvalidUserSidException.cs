using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLUserTableInvalidUserSidException : NavNCLTableConstraintBaseException
{
	public NavNCLUserTableInvalidUserSidException()
	{
	}

	public NavNCLUserTableInvalidUserSidException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLUserTableInvalidUserSidException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLUserTableInvalidUserSidException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLUserTableInvalidUserSidException Create(string sid, string name)
	{
		return new NavNCLUserTableInvalidUserSidException(string.Format(CultureInfo.CurrentCulture, Lang.UserSidIsInvalid, sid, name));
	}
}
