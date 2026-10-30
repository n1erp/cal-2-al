using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavPersonalizationStoreException : NavBaseException
{
	public NavPersonalizationStoreException()
	{
	}

	public NavPersonalizationStoreException(string message)
		: base(message)
	{
	}

	public NavPersonalizationStoreException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavPersonalizationStoreException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
