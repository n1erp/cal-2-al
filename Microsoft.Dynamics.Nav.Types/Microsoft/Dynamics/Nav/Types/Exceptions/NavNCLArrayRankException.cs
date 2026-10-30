using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLArrayRankException : NavNCLException
{
	public NavNCLArrayRankException()
	{
	}

	public NavNCLArrayRankException(string message)
		: base(message)
	{
	}

	public NavNCLArrayRankException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLArrayRankException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
