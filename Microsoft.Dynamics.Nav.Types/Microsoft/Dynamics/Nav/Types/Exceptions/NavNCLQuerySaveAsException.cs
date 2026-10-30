using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLQuerySaveAsException : NavNCLException
{
	public NavNCLQuerySaveAsException()
	{
	}

	public NavNCLQuerySaveAsException(string message)
		: base(message)
	{
	}

	public NavNCLQuerySaveAsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLQuerySaveAsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
