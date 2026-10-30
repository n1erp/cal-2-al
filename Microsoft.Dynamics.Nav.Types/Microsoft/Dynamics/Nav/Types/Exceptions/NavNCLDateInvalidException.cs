using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLDateInvalidException : NavNCLEvaluateException
{
	public NavNCLDateInvalidException()
		: base(Lang._00095_50019)
	{
	}

	public NavNCLDateInvalidException(string message)
		: base(message)
	{
	}

	public NavNCLDateInvalidException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLDateInvalidException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
