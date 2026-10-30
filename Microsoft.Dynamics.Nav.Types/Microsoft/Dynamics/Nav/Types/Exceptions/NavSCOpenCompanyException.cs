using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavSCOpenCompanyException : NavException
{
	public NavSCOpenCompanyException()
	{
	}

	public NavSCOpenCompanyException(string message)
		: base(message)
	{
	}

	public NavSCOpenCompanyException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavSCOpenCompanyException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
