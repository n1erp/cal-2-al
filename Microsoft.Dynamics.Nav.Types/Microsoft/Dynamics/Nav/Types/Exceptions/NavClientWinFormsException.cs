using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavClientWinFormsException : NavBaseException
{
	public NavClientWinFormsException()
	{
	}

	public NavClientWinFormsException(string message)
		: base(message)
	{
	}

	public NavClientWinFormsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavClientWinFormsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
