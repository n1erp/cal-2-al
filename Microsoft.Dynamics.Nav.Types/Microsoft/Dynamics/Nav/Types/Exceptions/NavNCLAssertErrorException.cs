using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLAssertErrorException : NavException
{
	public NavNCLAssertErrorException()
		: base(Lang.AssertError)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLAssertErrorException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLAssertErrorException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLAssertErrorException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
