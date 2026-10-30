using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Newtonsoft.Json.Linq;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidJsonTypeException : NavNCLException
{
	public NavNCLInvalidJsonTypeException()
	{
	}

	public NavNCLInvalidJsonTypeException(JTokenType type)
		: base(CreateMessage(type))
	{
	}

	public NavNCLInvalidJsonTypeException(string message)
		: base(message)
	{
	}

	protected NavNCLInvalidJsonTypeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavNCLInvalidJsonTypeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private static string CreateMessage(JTokenType type)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.NavJsonInvalidTokenType, type.ToString());
	}
}
