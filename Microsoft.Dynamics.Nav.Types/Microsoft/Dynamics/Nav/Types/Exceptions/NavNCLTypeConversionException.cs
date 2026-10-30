using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavNCLTypeConversionException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLTypeConversionException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLTypeConversionException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLTypeConversionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLTypeConversionException(NavType fromType, NavType toType)
		: base(CreateMessage(fromType, toType))
	{
	}

	public NavNCLTypeConversionException(NavType fromType, NavType toType, Exception innerException)
		: base(CreateMessage(fromType, toType), innerException)
	{
	}

	public NavNCLTypeConversionException(Type fromType, Type toType)
		: base(CreateMessage(fromType, toType))
	{
	}

	public NavNCLTypeConversionException(Type fromType, Type toType, Exception innerException)
		: base(CreateMessage(fromType, toType), innerException)
	{
	}

	private NavNCLTypeConversionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(object fromType, object toType)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang._00095_50168, fromType, toType);
	}
}
