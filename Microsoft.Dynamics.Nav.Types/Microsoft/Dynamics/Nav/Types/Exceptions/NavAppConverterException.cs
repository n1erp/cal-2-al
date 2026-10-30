using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavAppConverterException : NavAppException
{
	public NavAppConverterException()
	{
	}

	public NavAppConverterException(string message)
		: base(message)
	{
	}

	public NavAppConverterException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavAppConverterException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
