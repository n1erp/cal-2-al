using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLMissingMethodException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLMissingMethodException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLMissingMethodException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLMissingMethodException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLMissingMethodException(CultureInfo culture, int methodId)
		: base(CreateMessage(culture, methodId))
	{
	}

	public NavNCLMissingMethodException(Type type, string methodName)
		: base(CreateMessage(type, methodName))
	{
	}

	public NavNCLMissingMethodException(string fieldName, string methodName)
		: base(CreateMessage(fieldName, methodName))
	{
	}

	protected NavNCLMissingMethodException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(CultureInfo culture, int methodId)
	{
		return string.Format(culture, Lang.WrongReference, methodId, 0);
	}

	private static string CreateMessage(Type type, string methodName)
	{
		return new MissingMethodException(type.Name, methodName).Message;
	}

	private static string CreateMessage(string fieldName, string methodName)
	{
		return new MissingMethodException(fieldName, methodName).Message;
	}
}
