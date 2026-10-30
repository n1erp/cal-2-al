using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTestExplicitCommitNotAllowedException : NavTestBaseException
{
	public NavTestExplicitCommitNotAllowedException()
		: base(CreateMessage())
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTestExplicitCommitNotAllowedException(string message)
		: base(message)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTestExplicitCommitNotAllowedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTestExplicitCommitNotAllowedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage()
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.TestExplicitCommitNotAllowed);
	}
}
