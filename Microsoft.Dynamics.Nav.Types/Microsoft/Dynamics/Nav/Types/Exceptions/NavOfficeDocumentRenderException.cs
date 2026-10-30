using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavOfficeDocumentRenderException : NavException
{
	public NavOfficeDocumentRenderException()
	{
	}

	public NavOfficeDocumentRenderException(string message)
		: base(message)
	{
	}

	public NavOfficeDocumentRenderException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavOfficeDocumentRenderException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
