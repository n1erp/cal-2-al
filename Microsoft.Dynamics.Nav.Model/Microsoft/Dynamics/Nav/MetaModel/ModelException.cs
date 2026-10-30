using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.MetaModel;

public abstract class ModelException : TypeSystemException
{
	protected ModelException(string message, params object[] arguments)
		: base(message, arguments)
	{
	}

	protected ModelException()
	{
	}

	protected ModelException(string message)
		: base(message)
	{
	}

	protected ModelException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected ModelException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
