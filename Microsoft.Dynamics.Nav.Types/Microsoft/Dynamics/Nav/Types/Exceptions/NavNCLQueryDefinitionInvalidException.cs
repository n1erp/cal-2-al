using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLQueryDefinitionInvalidException : NavNCLException
{
	public NavNCLQueryDefinitionInvalidException()
	{
	}

	public NavNCLQueryDefinitionInvalidException(string message)
		: base(message)
	{
	}

	public NavNCLQueryDefinitionInvalidException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLQueryDefinitionInvalidException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLQueryDefinitionInvalidException Create(CultureInfo culture, int id, Exception innerException)
	{
		return new NavNCLQueryDefinitionInvalidException(string.Format(culture, Lang.InvalidQueryDefinition, id), innerException);
	}
}
