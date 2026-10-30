using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavObjectDefinitionChangedException : NavException
{
	public const int NavObjectDefinitionChangedExceptionErrorCode = 18023421;

	public NavObjectDefinitionChangedException()
	{
	}

	public NavObjectDefinitionChangedException(string message)
		: base(message)
	{
	}

	public NavObjectDefinitionChangedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavObjectDefinitionChangedException(CultureInfo culture, string tableCaption)
		: base(CreateMessage(culture, tableCaption))
	{
	}

	public NavObjectDefinitionChangedException(CultureInfo culture, string fieldCaption, NavType expectedType, NavType fieldType)
		: base(CreateMessage(culture, fieldCaption, expectedType, fieldType))
	{
	}

	public NavObjectDefinitionChangedException(CultureInfo culture, string objectCaption, ObjectType objectType)
		: base(CreateMessage(culture, objectCaption, objectType))
	{
	}

	protected NavObjectDefinitionChangedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(CultureInfo culture, string tableCaption)
	{
		return string.Format(culture, Lang.DefinitionOfKeyChanged, tableCaption);
	}

	private static string CreateMessage(CultureInfo culture, string fieldCaption, NavType expectedType, NavType fieldType)
	{
		return string.Format(culture, Lang.DefinitionOfFieldChanged, fieldCaption, expectedType, fieldType);
	}

	private static string CreateMessage(CultureInfo culture, string objectCaption, ObjectType objectType)
	{
		return string.Format(culture, Lang.ObjectDefinitionChanged, objectCaption, objectType);
	}
}
