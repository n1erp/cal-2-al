using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavInvalidColumnException : NavException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavInvalidColumnException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavInvalidColumnException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavInvalidColumnException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavInvalidColumnException(CultureInfo culture, string columnName, Exception innerException)
		: base(CreateMessage(culture, columnName), innerException)
	{
	}

	public NavInvalidColumnException(CultureInfo culture, string columnName)
		: base(CreateMessage(culture, columnName))
	{
	}

	protected NavInvalidColumnException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(IFormatProvider culture, string columnName)
	{
		return string.Format(culture, Lang.InvalidColumn, columnName);
	}
}
