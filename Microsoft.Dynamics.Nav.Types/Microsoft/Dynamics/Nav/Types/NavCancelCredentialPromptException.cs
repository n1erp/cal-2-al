using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class NavCancelCredentialPromptException : NavException
{
	private NavCancelCredentialPromptException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavCancelCredentialPromptException()
	{
	}

	public NavCancelCredentialPromptException(string message)
		: base(message)
	{
	}

	public NavCancelCredentialPromptException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
