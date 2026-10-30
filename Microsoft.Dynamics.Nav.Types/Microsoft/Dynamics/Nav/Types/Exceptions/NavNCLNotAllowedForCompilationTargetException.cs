using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLNotAllowedForCompilationTargetException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters")]
	public NavNCLNotAllowedForCompilationTargetException()
	{
	}

	public NavNCLNotAllowedForCompilationTargetException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLNotAllowedForCompilationTargetException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLNotAllowedForCompilationTargetException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
