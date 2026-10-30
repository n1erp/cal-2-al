using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLConnectionAlreadyOpenedException : NavNCLException
{
	public NavNCLConnectionAlreadyOpenedException()
		: base(Lang._00095_50016)
	{
	}

	public NavNCLConnectionAlreadyOpenedException(string message)
		: base(message)
	{
	}

	public NavNCLConnectionAlreadyOpenedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLConnectionAlreadyOpenedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
