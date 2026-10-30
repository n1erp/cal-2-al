using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavCompanyNotAccessibleException : NavException
{
	public NavCompanyNotAccessibleException()
	{
	}

	public NavCompanyNotAccessibleException(string message)
		: base(message)
	{
	}

	public NavCompanyNotAccessibleException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavCompanyNotAccessibleException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
