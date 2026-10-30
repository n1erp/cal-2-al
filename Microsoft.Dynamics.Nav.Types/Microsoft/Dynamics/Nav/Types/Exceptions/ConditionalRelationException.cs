using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class ConditionalRelationException : NavBaseException
{
	public ConditionalRelationException()
	{
	}

	public ConditionalRelationException(string message)
		: base(message)
	{
	}

	public ConditionalRelationException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected ConditionalRelationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
