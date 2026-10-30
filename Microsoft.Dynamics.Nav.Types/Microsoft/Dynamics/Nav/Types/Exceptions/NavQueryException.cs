using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavQueryException : NavException
{
	public NavQueryException()
	{
	}

	public NavQueryException(string message)
		: base(message)
	{
	}

	public NavQueryException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavQueryException(CultureInfo culture, string fieldCaption, NavType expectedType, NavType fieldType)
		: base(CreateMessage(culture, fieldCaption, expectedType, fieldType))
	{
	}

	public NavQueryException(CultureInfo culture, int fieldId)
		: base(CreateMessage(culture, fieldId))
	{
	}

	public NavQueryException(CultureInfo culture, string objectCaption)
		: base(CreateMessage(culture, objectCaption, NavObjectType.Query))
	{
	}

	protected NavQueryException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(CultureInfo culture, string fieldCaption, NavType expectedType, NavType fieldType)
	{
		return string.Format(culture, Lang.DefinitionOfColumnChanged, fieldCaption, expectedType, fieldType);
	}

	private static string CreateMessage(CultureInfo culture, int fieldId)
	{
		return string.Format(culture, Lang.ColumnRemoved, fieldId);
	}

	private static string CreateMessage(CultureInfo culture, string objectCaption, NavObjectType objectType)
	{
		return string.Format(culture, Lang.ObjectDefinitionChanged, objectCaption, objectType);
	}
}
