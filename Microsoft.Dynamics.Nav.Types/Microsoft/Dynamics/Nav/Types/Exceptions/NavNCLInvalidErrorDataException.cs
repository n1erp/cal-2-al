using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidErrorDataException : NavNCLException
{
	public NavNCLInvalidErrorDataException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidErrorDataException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidErrorDataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidErrorDataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
