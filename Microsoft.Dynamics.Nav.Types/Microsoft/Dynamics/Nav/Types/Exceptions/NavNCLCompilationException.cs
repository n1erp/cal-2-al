using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLCompilationException : NavNCLException
{
	[Obsolete("Do not use this constructor.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLCompilationException()
	{
	}

	[Obsolete("Do not use this constructor.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLCompilationException(string message)
		: base(message)
	{
	}

	public NavNCLCompilationException(CultureInfo culture, string errorFormat, params object[] parameters)
		: base(CreateMessage(culture, errorFormat, parameters))
	{
	}

	[Obsolete("Do not use this constructor.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLCompilationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLCompilationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(CultureInfo culture, string errorFormat, params object[] parameters)
	{
		return string.Format(culture, errorFormat, parameters);
	}
}
