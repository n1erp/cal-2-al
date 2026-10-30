using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLClosingDateOperationException : NavNCLException
{
	public NavNCLClosingDateOperationException()
		: base(Lang._00095_50027)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLClosingDateOperationException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLClosingDateOperationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLClosingDateOperationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
