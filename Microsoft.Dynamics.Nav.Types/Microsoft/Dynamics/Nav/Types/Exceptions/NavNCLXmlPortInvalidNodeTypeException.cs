using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortInvalidNodeTypeException : NavNCLException
{
	public NavNCLXmlPortInvalidNodeTypeException()
	{
	}

	public NavNCLXmlPortInvalidNodeTypeException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortInvalidNodeTypeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortInvalidNodeTypeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLXmlPortInvalidNodeTypeException CreateUnexpectedNodeType(CultureInfo culture, string nodeName)
	{
		return new NavNCLXmlPortInvalidNodeTypeException(string.Format(culture, Lang._00095_50102, nodeName));
	}
}
