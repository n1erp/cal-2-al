using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public abstract class NavBuilderException : NavException
{
	protected NavBuilderException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavBuilderException()
	{
	}

	protected NavBuilderException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	protected NavBuilderException(string message)
		: base(message)
	{
	}
}
