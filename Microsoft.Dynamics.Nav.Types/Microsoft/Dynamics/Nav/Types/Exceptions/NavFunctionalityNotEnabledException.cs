using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavFunctionalityNotEnabledException : NavNCLException
{
	[Obsolete("Do not use this constructor. Use the one with specific functionality parameters")]
	public NavFunctionalityNotEnabledException()
	{
	}

	public NavFunctionalityNotEnabledException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific functionality parameters")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavFunctionalityNotEnabledException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavFunctionalityNotEnabledException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
