using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class NavSqlConnectionLostException : NavSqlException
{
	public NavSqlConnectionLostException()
	{
	}

	public NavSqlConnectionLostException(string message)
		: base(message)
	{
	}

	public NavSqlConnectionLostException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavSqlConnectionLostException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
