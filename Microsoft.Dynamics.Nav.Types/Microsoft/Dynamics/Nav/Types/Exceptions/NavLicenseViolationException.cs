using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavLicenseViolationException : NavException
{
	public NavLicenseViolationException()
	{
	}

	public NavLicenseViolationException(string message)
		: base(message)
	{
	}

	public NavLicenseViolationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavLicenseViolationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
