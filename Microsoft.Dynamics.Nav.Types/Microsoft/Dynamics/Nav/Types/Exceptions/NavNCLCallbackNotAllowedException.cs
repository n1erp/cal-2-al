using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLCallbackNotAllowedException : NavNCLException
{
	public NavNCLCallbackNotAllowedException()
		: base(Lang._00095_50076)
	{
	}

	public NavNCLCallbackNotAllowedException(string message)
		: base(message)
	{
	}

	public NavNCLCallbackNotAllowedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLCallbackNotAllowedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
