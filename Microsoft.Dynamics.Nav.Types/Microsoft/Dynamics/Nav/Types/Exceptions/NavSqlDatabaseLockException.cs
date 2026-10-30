using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavSqlDatabaseLockException : NavBaseException
{
	public NavSqlDatabaseLockException()
	{
	}

	public NavSqlDatabaseLockException(string message)
		: base(message)
	{
	}

	public NavSqlDatabaseLockException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavSqlDatabaseLockException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
