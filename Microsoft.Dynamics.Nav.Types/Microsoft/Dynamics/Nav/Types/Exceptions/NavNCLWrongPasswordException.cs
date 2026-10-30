using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLWrongPasswordException : NavNCLException
{
	public NavNCLWrongPasswordException()
	{
	}

	public NavNCLWrongPasswordException(string message)
		: base(message)
	{
	}

	public NavNCLWrongPasswordException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLWrongPasswordException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLWrongPasswordException Create()
	{
		return new NavNCLWrongPasswordException(Lang.ExistingPasswordNotCorrect);
	}
}
