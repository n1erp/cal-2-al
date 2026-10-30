using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortUnexpectedAttributeException : NavNCLException
{
	public NavNCLXmlPortUnexpectedAttributeException()
	{
	}

	public NavNCLXmlPortUnexpectedAttributeException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortUnexpectedAttributeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortUnexpectedAttributeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLXmlPortUnexpectedAttributeException Create(CultureInfo culture, string attributeName)
	{
		return new NavNCLXmlPortUnexpectedAttributeException(string.Format(culture, Lang._00095_50109, attributeName));
	}
}
