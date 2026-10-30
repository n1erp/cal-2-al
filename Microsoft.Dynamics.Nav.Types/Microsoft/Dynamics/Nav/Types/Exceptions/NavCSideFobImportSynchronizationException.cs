using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavCSideFobImportSynchronizationException : NavCSideDataException
{
	public NavCSideFobImportSynchronizationException()
	{
	}

	public NavCSideFobImportSynchronizationException(string message)
		: base(message)
	{
	}

	public NavCSideFobImportSynchronizationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavCSideFobImportSynchronizationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavCSideFobImportSynchronizationException(int errorCode, string message)
		: base(errorCode, message)
	{
	}
}
