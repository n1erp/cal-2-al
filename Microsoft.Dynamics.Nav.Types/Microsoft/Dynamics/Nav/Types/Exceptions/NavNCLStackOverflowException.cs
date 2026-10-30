using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLStackOverflowException : NavNCLException
{
	public NavNCLStackOverflowException()
		: base(Lang.StackOverflow)
	{
	}

	public NavNCLStackOverflowException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLStackOverflowException(string message)
		: base(message)
	{
	}

	protected NavNCLStackOverflowException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
