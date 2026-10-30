using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTestRowNotFoundException : NavTestBaseException
{
	public NavTestRowNotFoundException()
		: base(CreateMessage())
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTestRowNotFoundException(string message)
		: base(message)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTestRowNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected NavTestRowNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage()
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.TestPageRowNotFound);
	}
}
