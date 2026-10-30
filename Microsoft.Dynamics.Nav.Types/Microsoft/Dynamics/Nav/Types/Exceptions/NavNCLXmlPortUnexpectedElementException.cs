using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortUnexpectedElementException : NavNCLException
{
	public NavNCLXmlPortUnexpectedElementException()
	{
	}

	public NavNCLXmlPortUnexpectedElementException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortUnexpectedElementException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortUnexpectedElementException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLXmlPortUnexpectedElementException Create(CultureInfo culture, string elementName)
	{
		return new NavNCLXmlPortUnexpectedElementException(string.Format(culture, Lang._00095_50110, elementName));
	}
}
