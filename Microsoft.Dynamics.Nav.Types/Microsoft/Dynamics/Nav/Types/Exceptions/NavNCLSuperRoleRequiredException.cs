using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLSuperRoleRequiredException : NavNCLException
{
	public NavNCLSuperRoleRequiredException()
	{
	}

	public NavNCLSuperRoleRequiredException(string message)
		: base(message)
	{
	}

	public NavNCLSuperRoleRequiredException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLSuperRoleRequiredException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLSuperRoleRequiredException Create()
	{
		return new NavNCLSuperRoleRequiredException(Lang.SuperRoleRequired);
	}
}
