using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidStreamModeException : NavNCLException
{
	public NavNCLInvalidStreamModeException()
	{
	}

	public NavNCLInvalidStreamModeException(string message)
		: base(message)
	{
	}

	public NavNCLInvalidStreamModeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidStreamModeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLInvalidStreamModeException CreateInvalidReadMode(CultureInfo culture, string modeName)
	{
		return new NavNCLInvalidStreamModeException(string.Format(culture, Lang.ALStreamReadModeError, modeName));
	}

	public static NavNCLInvalidStreamModeException CreateInvalidWriteMode(CultureInfo culture, string modeName)
	{
		return new NavNCLInvalidStreamModeException(string.Format(culture, Lang.ALStreamWriteModeError, modeName));
	}
}
