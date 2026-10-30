using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLNotSupportedTypeException : NavNCLException
{
	public NavNCLNotSupportedTypeException()
	{
	}

	public NavNCLNotSupportedTypeException(string message)
		: base(message)
	{
	}

	public NavNCLNotSupportedTypeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLNotSupportedTypeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLNotSupportedTypeException CreateSplitKeyType(CultureInfo culture, string typeName)
	{
		return new NavNCLNotSupportedTypeException(string.Format(culture, Lang.FormUnsupportedSplitKeyType, typeName));
	}

	public static NavNCLNotSupportedTypeException CreateArrayCloneNotSupportedType(CultureInfo culture, string typeName)
	{
		return new NavNCLNotSupportedTypeException(string.Format(culture, Lang.ArrayCloneIllegalType, typeName));
	}

	public static NavNCLNotSupportedTypeException CreateUnknownType(CultureInfo culture, string typeName)
	{
		return new NavNCLNotSupportedTypeException(string.Format(culture, Lang._00095_50017, typeName));
	}

	public static NavNCLNotSupportedTypeException CreateUnsupportedGetType(CultureInfo culture, string typeName)
	{
		return new NavNCLNotSupportedTypeException(string.Format(culture, Lang.GetDotNetType, typeName));
	}

	public static NavNCLNotSupportedTypeException CreateReadFromBytesNotSupportedType(CultureInfo culture, NavType type)
	{
		return new NavNCLNotSupportedTypeException(string.Format(culture, "The supplied {0} is not supported when reading from a byte[].", type));
	}

	public static NavNCLNotSupportedTypeException CreateUnsupportedType(CultureInfo culture, string typeName)
	{
		return new NavNCLNotSupportedTypeException(string.Format(culture, Lang.NavTypeIsNotSupported, typeName));
	}
}
