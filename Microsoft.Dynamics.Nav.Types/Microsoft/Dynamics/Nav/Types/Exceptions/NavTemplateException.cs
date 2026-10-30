using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTemplateException : NavBaseException
{
	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTemplateException()
		: base(CreateMessage(CultureInfo.InvariantCulture))
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTemplateException(string message)
		: base(message)
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavTemplateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTemplateException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(CultureInfo culture)
	{
		return string.Format(culture, "");
	}
}
