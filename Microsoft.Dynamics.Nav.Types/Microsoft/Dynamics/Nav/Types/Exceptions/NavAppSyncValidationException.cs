using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavAppSyncValidationException : NavAppSyncException
{
	public NavAppSyncValidationException()
	{
	}

	public NavAppSyncValidationException(string message)
		: base(message)
	{
	}

	public NavAppSyncValidationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavAppSyncValidationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
