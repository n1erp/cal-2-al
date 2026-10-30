using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavUnknownNCLDocumentServiceException : NavNCLDocumentServiceException
{
	public NavUnknownNCLDocumentServiceException()
	{
	}

	public NavUnknownNCLDocumentServiceException(string message)
		: base(message)
	{
	}

	public NavUnknownNCLDocumentServiceException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavUnknownNCLDocumentServiceException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
