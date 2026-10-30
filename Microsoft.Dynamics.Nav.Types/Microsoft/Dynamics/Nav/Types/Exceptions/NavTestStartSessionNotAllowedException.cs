using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTestStartSessionNotAllowedException : NavTestBaseException
{
	public NavTestStartSessionNotAllowedException()
		: base(CreateMessage())
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTestStartSessionNotAllowedException(string message)
		: base(message)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTestStartSessionNotAllowedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTestStartSessionNotAllowedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage()
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.TestStartSessionNotAllowed);
	}
}
