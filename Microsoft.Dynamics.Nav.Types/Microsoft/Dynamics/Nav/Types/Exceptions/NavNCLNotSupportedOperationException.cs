using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLNotSupportedOperationException : NavNCLException
{
	public NavNCLNotSupportedOperationException()
		: base(Lang._00095_50067)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLNotSupportedOperationException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters or the default one instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLNotSupportedOperationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLNotSupportedOperationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
