using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortDataException : NavNCLException
{
	public NavNCLXmlPortDataException()
	{
	}

	public NavNCLXmlPortDataException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortDataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortDataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLXmlPortDataException Create(CultureInfo culture, string nodeName)
	{
		return new NavNCLXmlPortDataException(string.Format(culture, Lang._00095_50115, nodeName));
	}
}
