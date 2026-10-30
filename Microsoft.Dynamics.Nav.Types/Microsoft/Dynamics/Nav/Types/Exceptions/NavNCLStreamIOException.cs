using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLStreamIOException : NavNCLException
{
	public NavNCLStreamIOException()
		: base(Lang._00095_50046)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLStreamIOException(string message)
		: base(message)
	{
	}

	public NavNCLStreamIOException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLStreamIOException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
