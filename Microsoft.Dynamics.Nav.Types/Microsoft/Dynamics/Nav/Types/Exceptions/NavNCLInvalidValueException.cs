using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidValueException : NavNCLException
{
	public NavNCLInvalidValueException()
		: base(Lang._00095_50018)
	{
	}

	public NavNCLInvalidValueException(string message)
		: base(message)
	{
	}

	public NavNCLInvalidValueException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidValueException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
