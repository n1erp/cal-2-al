using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavDocumentServiceNotConfiguredException : NavException
{
	public NavDocumentServiceNotConfiguredException()
	{
	}

	public NavDocumentServiceNotConfiguredException(string message)
		: base(message)
	{
	}

	public NavDocumentServiceNotConfiguredException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavDocumentServiceNotConfiguredException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
