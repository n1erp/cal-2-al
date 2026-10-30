using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlNodeNotFoundException : NavNCLException
{
	public NavNCLXmlNodeNotFoundException()
		: base(Lang.NavXmlNodeNotSet)
	{
	}

	public NavNCLXmlNodeNotFoundException(string nodeName)
		: base(CreateErrorMessage(nodeName))
	{
	}

	public NavNCLXmlNodeNotFoundException(string localName, string ns)
		: base(CreateErrorMessage(string.Format(CultureInfo.InvariantCulture, "{{{0}}}{1}", ns, localName)))
	{
	}

	protected NavNCLXmlNodeNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavNCLXmlNodeNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private static string CreateErrorMessage(string name)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.NavXmlNodeWithNameNotFound, name);
	}
}
