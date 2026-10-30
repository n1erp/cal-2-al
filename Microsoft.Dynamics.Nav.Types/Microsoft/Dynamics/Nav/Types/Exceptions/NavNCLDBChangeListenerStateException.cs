using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLDBChangeListenerStateException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLDBChangeListenerStateException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLDBChangeListenerStateException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLDBChangeListenerStateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLDBChangeListenerStateException(CultureInfo culture, string info)
		: base(CreateMessage(culture, info))
	{
	}

	protected NavNCLDBChangeListenerStateException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(CultureInfo culture, string info)
	{
		return string.Format(culture, Lang.ChangeListenerUnexpectedState, info);
	}
}
