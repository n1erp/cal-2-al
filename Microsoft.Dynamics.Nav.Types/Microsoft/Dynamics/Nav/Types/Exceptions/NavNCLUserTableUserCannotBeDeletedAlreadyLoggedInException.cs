using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLUserTableUserCannotBeDeletedAlreadyLoggedInException : NavNCLTableConstraintBaseException
{
	public NavNCLUserTableUserCannotBeDeletedAlreadyLoggedInException()
	{
	}

	public NavNCLUserTableUserCannotBeDeletedAlreadyLoggedInException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLUserTableUserCannotBeDeletedAlreadyLoggedInException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLUserTableUserCannotBeDeletedAlreadyLoggedInException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLUserTableUserCannotBeDeletedAlreadyLoggedInException Create(string userName)
	{
		return new NavNCLUserTableUserCannotBeDeletedAlreadyLoggedInException(string.Format(CultureInfo.CurrentCulture, Lang.UserCannotBeDeletedAlreadyLoggedIn, userName));
	}
}
