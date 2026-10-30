using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNclDuplicateJsonPropertyException : NavNCLException
{
	public NavNclDuplicateJsonPropertyException()
	{
	}

	public NavNclDuplicateJsonPropertyException(string propertyKey)
		: base(CreateMessage(propertyKey))
	{
	}

	protected NavNclDuplicateJsonPropertyException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavNclDuplicateJsonPropertyException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private static string CreateMessage(string propertyKey)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.NavJsonDuplicateProperty, propertyKey);
	}
}
