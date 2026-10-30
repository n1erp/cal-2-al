using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavDialogUpdateAbortException : NavException
{
	public NavDialogUpdateAbortException()
	{
	}

	public NavDialogUpdateAbortException(string message)
		: base(message)
	{
	}

	public NavDialogUpdateAbortException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavDialogUpdateAbortException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavDialogUpdateAbortException Create()
	{
		return new NavDialogUpdateAbortException(string.Empty, null)
		{
			SuppressMessage = true
		};
	}
}
