using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLDotNetControlAddInNotCreatedException : NavNCLException
{
	public NavNCLDotNetControlAddInNotCreatedException()
	{
	}

	public NavNCLDotNetControlAddInNotCreatedException(string message)
		: base(message)
	{
	}

	public NavNCLDotNetControlAddInNotCreatedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLDotNetControlAddInNotCreatedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
