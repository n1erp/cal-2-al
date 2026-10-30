using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortDuplicateElementException : NavNCLException
{
	public NavNCLXmlPortDuplicateElementException()
	{
	}

	public NavNCLXmlPortDuplicateElementException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortDuplicateElementException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortDuplicateElementException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLXmlPortDuplicateElementException Create(CultureInfo culture, string elementName)
	{
		return new NavNCLXmlPortDuplicateElementException(string.Format(culture, Lang._00095_50114, elementName));
	}
}
