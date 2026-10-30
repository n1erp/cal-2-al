using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlException : NavNCLException
{
	public NavNCLXmlException()
	{
	}

	public NavNCLXmlException(string message)
		: base(message)
	{
	}

	public NavNCLXmlException(string message, int lineNumber, int linePosition)
		: this(string.Format(CultureInfo.CurrentCulture, Lang.XmlException, message, lineNumber, linePosition))
	{
	}

	public NavNCLXmlException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
