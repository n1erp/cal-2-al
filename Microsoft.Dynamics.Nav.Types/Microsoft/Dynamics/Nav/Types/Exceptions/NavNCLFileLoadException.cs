using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLFileLoadException : NavNCLException
{
	public NavNCLFileLoadException()
	{
	}

	public NavNCLFileLoadException(string message)
		: base(message)
	{
	}

	public NavNCLFileLoadException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLFileLoadException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
