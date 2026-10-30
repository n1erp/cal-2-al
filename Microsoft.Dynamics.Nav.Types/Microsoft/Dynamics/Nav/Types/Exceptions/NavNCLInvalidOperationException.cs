using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidOperationException : NavNCLException
{
	public NavNCLInvalidOperationException()
		: base(Lang.InvalidOperation)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidOperationException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLInvalidOperationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidOperationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
