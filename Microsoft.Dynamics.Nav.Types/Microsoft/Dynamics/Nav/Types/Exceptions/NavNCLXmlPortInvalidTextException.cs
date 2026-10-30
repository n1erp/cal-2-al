using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortInvalidTextException : NavNCLException
{
	public NavNCLXmlPortInvalidTextException()
		: base(Lang._00095_50125)
	{
	}

	public NavNCLXmlPortInvalidTextException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortInvalidTextException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortInvalidTextException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
