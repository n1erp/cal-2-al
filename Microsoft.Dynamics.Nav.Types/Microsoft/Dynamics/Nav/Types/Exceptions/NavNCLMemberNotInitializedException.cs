using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLMemberNotInitializedException : NavNCLException
{
	public NavNCLMemberNotInitializedException()
	{
	}

	public NavNCLMemberNotInitializedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLMemberNotInitializedException(string message)
		: base(message)
	{
	}

	protected NavNCLMemberNotInitializedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
