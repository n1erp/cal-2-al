using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class InvalidHomepageException : NavMetadataException
{
	private InvalidHomepageException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public InvalidHomepageException()
	{
	}

	public InvalidHomepageException(string message)
		: base(message)
	{
	}

	public InvalidHomepageException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
