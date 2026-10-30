using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavClientFileDialogException : NavClientBaseException
{
	public NavClientFileDialogException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavClientFileDialogException()
	{
	}

	protected NavClientFileDialogException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavClientFileDialogException(string message)
		: base(message)
	{
	}
}
