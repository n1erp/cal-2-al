using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public abstract class TypeSystemException : Exception
{
	public TypeSystemException()
	{
	}

	protected TypeSystemException(string message)
		: base(message)
	{
	}

	protected TypeSystemException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected TypeSystemException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	protected TypeSystemException(string message, params object[] arguments)
		: base(string.Format(CultureInfo.InvariantCulture, message, arguments))
	{
	}

	protected TypeSystemException(Exception innerException, string message, params object[] arguments)
		: base(string.Format(message, arguments), innerException)
	{
	}
}
