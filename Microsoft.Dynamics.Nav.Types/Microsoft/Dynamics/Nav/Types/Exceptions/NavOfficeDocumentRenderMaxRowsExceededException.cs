using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavOfficeDocumentRenderMaxRowsExceededException : NavOfficeDocumentRenderException
{
	public NavOfficeDocumentRenderMaxRowsExceededException()
	{
	}

	public NavOfficeDocumentRenderMaxRowsExceededException(string message)
		: base(message)
	{
	}

	public NavOfficeDocumentRenderMaxRowsExceededException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavOfficeDocumentRenderMaxRowsExceededException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
