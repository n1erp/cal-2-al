using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

[Serializable]
public class NavDesignerUnexpectedValueException : NavAppException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavDesignerUnexpectedValueException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavDesignerUnexpectedValueException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavDesignerUnexpectedValueException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	protected NavDesignerUnexpectedValueException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavDesignerUnexpectedValueException(CultureInfo culture, string value, string type)
		: base(CreateMessage(culture, value, type))
	{
	}

	private static string CreateMessage(CultureInfo culture, string value, string type)
	{
		return string.Format(culture, Lang.DesignerUnexpectedValue, value, type);
	}
}
