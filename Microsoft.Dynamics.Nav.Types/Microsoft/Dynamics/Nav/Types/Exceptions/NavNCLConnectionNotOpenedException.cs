using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLConnectionNotOpenedException : NavNCLException
{
	public NavNCLConnectionNotOpenedException()
		: base(Lang._00095_50003)
	{
	}

	public NavNCLConnectionNotOpenedException(string message)
		: base(message)
	{
	}

	public NavNCLConnectionNotOpenedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLConnectionNotOpenedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
