using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavNCLActiveSessionsMismatchException : NavNCLException
{
	public NavNCLActiveSessionsMismatchException()
		: base(Lang.ActiveSessionsMismatch)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLActiveSessionsMismatchException(string message)
		: base(message)
	{
	}

	[Obsolete("Only use the constructor taking an existing exception")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLActiveSessionsMismatchException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	[Obsolete("Only use the constructor taking an existing exception")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLActiveSessionsMismatchException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
