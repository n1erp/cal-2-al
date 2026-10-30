using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.MetaModel;

public class GenericElementException : ModelException
{
	internal GenericElementException(string message, params object[] arguments)
		: base(message, arguments)
	{
	}

	public GenericElementException()
	{
	}

	public GenericElementException(string message)
		: base(message)
	{
	}

	public GenericElementException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected GenericElementException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
