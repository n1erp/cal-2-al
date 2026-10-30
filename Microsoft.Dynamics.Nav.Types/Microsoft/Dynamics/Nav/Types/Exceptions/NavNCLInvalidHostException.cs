using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidHostException : NavNCLException
{
	public NavNCLInvalidHostException()
		: base(Lang.InvalidStreamOrBlobSource)
	{
	}

	public NavNCLInvalidHostException(string message)
		: base(message)
	{
	}

	public NavNCLInvalidHostException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidHostException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
