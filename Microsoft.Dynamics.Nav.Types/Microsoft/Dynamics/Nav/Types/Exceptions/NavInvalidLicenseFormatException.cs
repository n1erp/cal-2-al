using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavInvalidLicenseFormatException : NavException
{
	public NavInvalidLicenseFormatException()
	{
	}

	public NavInvalidLicenseFormatException(string message)
		: base(message)
	{
	}

	public NavInvalidLicenseFormatException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavInvalidLicenseFormatException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
