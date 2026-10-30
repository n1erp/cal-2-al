using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Runtime;

[Serializable]
public class DeadSqlConnectionException : Exception
{
	public DeadSqlConnectionException()
	{
	}

	public DeadSqlConnectionException(string message)
		: base(message)
	{
	}

	public DeadSqlConnectionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected DeadSqlConnectionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
