using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNclCreateUrlException : NavNCLException
{
	public NavNclCreateUrlException()
	{
	}

	public NavNclCreateUrlException(string message)
		: base(message)
	{
	}

	public NavNclCreateUrlException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNclCreateUrlException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
