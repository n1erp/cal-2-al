using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLClientCultureInfoException : NavNCLException
{
	public NavNCLClientCultureInfoException()
		: base("The supplied regional settings are not valid.")
	{
	}

	public NavNCLClientCultureInfoException(string message)
		: base(message)
	{
	}

	public NavNCLClientCultureInfoException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLClientCultureInfoException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
