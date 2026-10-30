using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLPageAlreadyInitializedException : NavNCLException
{
	public NavNCLPageAlreadyInitializedException()
		: base(Lang.PageAlreadyInitialized)
	{
	}

	public NavNCLPageAlreadyInitializedException(string message)
		: base(message)
	{
	}

	public NavNCLPageAlreadyInitializedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLPageAlreadyInitializedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
