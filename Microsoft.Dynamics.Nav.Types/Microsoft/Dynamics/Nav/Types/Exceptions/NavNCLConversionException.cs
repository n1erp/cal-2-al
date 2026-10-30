using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavNCLConversionException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLConversionException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLConversionException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLConversionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLConversionException(NavType fromTypeName, NavType toTypeName)
		: base(CreateMessage(fromTypeName, toTypeName))
	{
	}

	public NavNCLConversionException(Type fromType, Type toType)
		: base(CreateMessage(fromType, toType))
	{
	}

	private NavNCLConversionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(object fromType, object toType)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang._00095_50167, fromType, toType);
	}
}
