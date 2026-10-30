using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortNodeProcessException : NavNCLException
{
	public NavNCLXmlPortNodeProcessException()
	{
	}

	public NavNCLXmlPortNodeProcessException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortNodeProcessException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortNodeProcessException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLXmlPortNodeProcessException Create(CultureInfo culture, string nodeName, Exception innerException)
	{
		string arg = ((innerException != null && !string.IsNullOrWhiteSpace(innerException.Message)) ? innerException.Message : string.Empty);
		return new NavNCLXmlPortNodeProcessException(string.Format(culture, Lang._00095_50101, nodeName, arg), innerException);
	}
}
