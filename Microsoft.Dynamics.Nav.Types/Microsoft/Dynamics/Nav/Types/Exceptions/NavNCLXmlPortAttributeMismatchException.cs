using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortAttributeMismatchException : NavNCLException
{
	public NavNCLXmlPortAttributeMismatchException()
	{
	}

	public NavNCLXmlPortAttributeMismatchException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortAttributeMismatchException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortAttributeMismatchException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLXmlPortAttributeMismatchException Create(CultureInfo culture, string expectedAttribute)
	{
		return new NavNCLXmlPortAttributeMismatchException(string.Format(culture, Lang._00095_50108, expectedAttribute));
	}
}
