using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

[Serializable]
internal class NavDesignerExtensionStorageException : NavAppException
{
	public NavDesignerExtensionStorageException()
	{
	}

	public NavDesignerExtensionStorageException(string message)
		: base(message)
	{
	}

	public NavDesignerExtensionStorageException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavDesignerExtensionStorageException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
