using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavSqlDatabaseSyncException : NavBaseException
{
	public NavSqlDatabaseSyncException()
	{
	}

	public NavSqlDatabaseSyncException(string message)
		: base(message)
	{
	}

	public NavSqlDatabaseSyncException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavSqlDatabaseSyncException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
