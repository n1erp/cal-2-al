using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortInvalidXmlException : NavNCLException
{
	public NavNCLXmlPortInvalidXmlException()
	{
	}

	public NavNCLXmlPortInvalidXmlException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortInvalidXmlException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortInvalidXmlException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLXmlPortInvalidXmlException Create(CultureInfo culture, string schemaValidationMessage)
	{
		return new NavNCLXmlPortInvalidXmlException(string.Format(culture, Lang._00095_50117, schemaValidationMessage));
	}
}
