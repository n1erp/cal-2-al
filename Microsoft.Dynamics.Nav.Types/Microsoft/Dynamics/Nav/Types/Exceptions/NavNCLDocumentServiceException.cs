using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLDocumentServiceException : NavException
{
	public NavNCLDocumentServiceException()
	{
	}

	public NavNCLDocumentServiceException(string message)
		: base(message)
	{
	}

	public NavNCLDocumentServiceException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLDocumentServiceException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
