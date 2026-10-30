using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLXmlPortImportException : NavNCLException
{
	public NavNCLXmlPortImportException()
	{
	}

	public NavNCLXmlPortImportException(string message)
		: base(message)
	{
	}

	public NavNCLXmlPortImportException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLXmlPortImportException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
