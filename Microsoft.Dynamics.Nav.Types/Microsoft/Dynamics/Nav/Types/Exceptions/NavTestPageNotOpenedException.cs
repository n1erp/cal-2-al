using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTestPageNotOpenedException : NavTestBaseException
{
	public NavTestPageNotOpenedException()
		: base(CreateMessage())
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTestPageNotOpenedException(string message)
		: base(message)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTestPageNotOpenedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTestPageNotOpenedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage()
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.TestPageNotOpened);
	}
}
