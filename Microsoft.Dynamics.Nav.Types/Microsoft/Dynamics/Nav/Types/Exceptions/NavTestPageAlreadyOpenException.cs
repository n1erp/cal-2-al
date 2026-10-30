using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTestPageAlreadyOpenException : NavTestBaseException
{
	public NavTestPageAlreadyOpenException()
		: base(CreateMessage())
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTestPageAlreadyOpenException(string message)
		: base(message)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTestPageAlreadyOpenException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTestPageAlreadyOpenException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage()
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.TestPageAlreadyOpen);
	}
}
