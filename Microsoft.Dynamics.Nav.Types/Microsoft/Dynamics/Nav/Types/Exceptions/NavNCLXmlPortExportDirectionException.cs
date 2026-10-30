using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortExportDirectionException : NavNCLException
{
	public NavNCLXmlPortExportDirectionException()
	{
	}

	public NavNCLXmlPortExportDirectionException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortExportDirectionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortExportDirectionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLXmlPortExportDirectionException Create(CultureInfo culture, string portName)
	{
		return new NavNCLXmlPortExportDirectionException(string.Format(culture, Lang._00095_50112, portName));
	}
}
