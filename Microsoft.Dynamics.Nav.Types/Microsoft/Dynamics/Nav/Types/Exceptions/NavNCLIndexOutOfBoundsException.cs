using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLIndexOutOfBoundsException : NavNCLException
{
	public NavNCLIndexOutOfBoundsException()
		: base(Lang._00095_50035)
	{
	}

	public NavNCLIndexOutOfBoundsException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLIndexOutOfBoundsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLIndexOutOfBoundsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
