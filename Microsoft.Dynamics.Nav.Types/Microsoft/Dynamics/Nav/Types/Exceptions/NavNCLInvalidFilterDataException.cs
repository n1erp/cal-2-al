using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidFilterDataException : NavNCLException
{
	public NavNCLInvalidFilterDataException()
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidFilterDataException(string message)
		: base(message)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidFilterDataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidFilterDataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
