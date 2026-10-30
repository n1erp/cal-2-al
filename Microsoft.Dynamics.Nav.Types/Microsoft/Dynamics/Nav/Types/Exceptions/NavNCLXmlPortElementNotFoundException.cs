using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortElementNotFoundException : NavNCLException
{
	public NavNCLXmlPortElementNotFoundException()
	{
	}

	public NavNCLXmlPortElementNotFoundException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortElementNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortElementNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLXmlPortElementNotFoundException Create(CultureInfo culture, string elementName)
	{
		return new NavNCLXmlPortElementNotFoundException(string.Format(culture, Lang._00095_50106, elementName));
	}
}
