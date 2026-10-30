using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortTypeSizeExceededException : NavNCLException
{
	public NavNCLXmlPortTypeSizeExceededException()
	{
	}

	public NavNCLXmlPortTypeSizeExceededException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortTypeSizeExceededException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortTypeSizeExceededException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLXmlPortTypeSizeExceededException Create(CultureInfo culture, string nodeValue, int valueLength, string nodeType)
	{
		return new NavNCLXmlPortTypeSizeExceededException(string.Format(culture, Lang._00095_50113, nodeValue, valueLength, nodeType));
	}
}
