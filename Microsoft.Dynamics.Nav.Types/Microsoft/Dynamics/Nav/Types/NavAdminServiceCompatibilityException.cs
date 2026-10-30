using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
internal class NavAdminServiceCompatibilityException : NavBaseException
{
	public NavAdminServiceCompatibilityException(string message)
		: base(message)
	{
	}

	public NavAdminServiceCompatibilityException(string message, params object[] args)
		: this(string.Format(CultureInfo.CurrentCulture, message, args))
	{
	}

	public NavAdminServiceCompatibilityException(string message, Exception exception)
		: base(message, exception)
	{
	}

	protected NavAdminServiceCompatibilityException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}

	public NavAdminServiceCompatibilityException()
	{
	}
}
