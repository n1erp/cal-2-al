using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavAppSyncException : NavAppException
{
	public NavAppSyncException()
	{
	}

	public NavAppSyncException(string message)
		: base(message)
	{
	}

	public NavAppSyncException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavAppSyncException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
