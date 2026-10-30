using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavCompanyNotFoundException : NavBaseException
{
	public NavCompanyNotFoundException()
	{
	}

	public NavCompanyNotFoundException(string message)
		: base(message)
	{
	}

	public NavCompanyNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavCompanyNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
