using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

[Serializable]
public class NavLicenseServiceException : NavException
{
	public NavLicenseServiceException()
	{
	}

	public NavLicenseServiceException(string message)
		: base(message)
	{
	}

	protected NavLicenseServiceException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavLicenseServiceException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
