using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortImportDirectionException : NavNCLException
{
	public NavNCLXmlPortImportDirectionException()
	{
	}

	public NavNCLXmlPortImportDirectionException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortImportDirectionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortImportDirectionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLXmlPortImportDirectionException Create(CultureInfo culture, string portName)
	{
		return new NavNCLXmlPortImportDirectionException(string.Format(culture, Lang._00095_50111, portName));
	}
}
