using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortMetadataException : NavNCLException
{
	public NavNCLXmlPortMetadataException()
		: base(Lang._00095_50105)
	{
	}

	public NavNCLXmlPortMetadataException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortMetadataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortMetadataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLXmlPortMetadataException CreateOccurrenceExceeded(CultureInfo culture, string expectedElement, string occurrence, string foundElement)
	{
		return new NavNCLXmlPortMetadataException(string.Format(culture, Lang._00095_50107, expectedElement, occurrence, foundElement));
	}
}
