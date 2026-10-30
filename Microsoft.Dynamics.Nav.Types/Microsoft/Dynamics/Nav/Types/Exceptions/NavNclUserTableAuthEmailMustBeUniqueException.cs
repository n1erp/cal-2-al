using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNclUserTableAuthEmailMustBeUniqueException : NavNCLTableConstraintBaseException
{
	public NavNclUserTableAuthEmailMustBeUniqueException()
	{
	}

	public NavNclUserTableAuthEmailMustBeUniqueException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNclUserTableAuthEmailMustBeUniqueException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNclUserTableAuthEmailMustBeUniqueException(CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(culture, message, args)
	{
	}

	protected NavNclUserTableAuthEmailMustBeUniqueException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNclUserTableAuthEmailMustBeUniqueException Create(string userEmail)
	{
		return new NavNclUserTableAuthEmailMustBeUniqueException(CultureInfo.CurrentCulture, Lang.AuthenticationEmailIsUsedByAnotherUser, DiagnosticParameter.CustomerContent(userEmail));
	}
}
