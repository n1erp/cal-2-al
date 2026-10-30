using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLTableIdMismatchException : NavNCLException
{
	public NavNCLTableIdMismatchException()
		: base(Lang._00095_50039)
	{
	}

	public NavNCLTableIdMismatchException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLTableIdMismatchException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLTableIdMismatchException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
