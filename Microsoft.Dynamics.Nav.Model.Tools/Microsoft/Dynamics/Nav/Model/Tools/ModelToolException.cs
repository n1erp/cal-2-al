using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

public class ModelToolException : TypeSystemException
{
	internal ModelToolException(string message, params object[] arguments)
		: base(message, arguments)
	{
	}

	public ModelToolException()
	{
	}

	public ModelToolException(string message)
		: base(message)
	{
	}

	public ModelToolException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected ModelToolException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
