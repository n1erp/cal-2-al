using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavInvalidSortingFieldsException : NavBaseException
{
	public NavInvalidSortingFieldsException()
	{
	}

	public NavInvalidSortingFieldsException(string message)
		: base(message)
	{
	}

	public NavInvalidSortingFieldsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavInvalidSortingFieldsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
