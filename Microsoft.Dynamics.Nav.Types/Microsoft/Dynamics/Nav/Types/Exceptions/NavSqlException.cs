using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavSqlException : NavBaseException
{
	public int ErrorNumber { get; private set; }

	public bool TransactionRolledBack { get; private set; }

	public NavSqlException()
	{
	}

	public NavSqlException(string message)
		: base(message)
	{
	}

	public NavSqlException(string message, bool transactionRolledBack)
		: base(message)
	{
		TransactionRolledBack = transactionRolledBack;
	}

	public NavSqlException(string message, int errorNumber)
		: base(message)
	{
		ErrorNumber = errorNumber;
	}

	public NavSqlException(string message, int errorNumber, bool transactionRolledBack)
		: this(message, errorNumber)
	{
		TransactionRolledBack = transactionRolledBack;
	}

	public NavSqlException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavSqlException(string message, Exception innerException, int errorNumber)
		: base(message, innerException)
	{
		ErrorNumber = errorNumber;
	}

	public NavSqlException(string message, string diagnosticMessage, Exception innerException, int errorNumber)
		: base(message, diagnosticMessage, innerException)
	{
		ErrorNumber = errorNumber;
	}

	public NavSqlException(string message, Exception innerException, int errorNumber, bool transactionRolledBack)
		: base(message, innerException)
	{
		ErrorNumber = errorNumber;
		TransactionRolledBack = transactionRolledBack;
	}

	protected NavSqlException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ErrorNumber", ErrorNumber);
	}
}
