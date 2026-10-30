using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLJsonPropertyNotFoundException : NavNCLException
{
	public NavNCLJsonPropertyNotFoundException()
	{
	}

	public NavNCLJsonPropertyNotFoundException(string propertyKey)
		: base(CreateMessage(propertyKey))
	{
	}

	protected NavNCLJsonPropertyNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavNCLJsonPropertyNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private static string CreateMessage(string propertyKey)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.NavJsonPropertyNotFound, propertyKey);
	}
}
