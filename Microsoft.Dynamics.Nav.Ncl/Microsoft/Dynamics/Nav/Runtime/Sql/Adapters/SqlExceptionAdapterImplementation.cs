using System;
using System.Data.SqlClient;

namespace Microsoft.Dynamics.Nav.Runtime.Sql.Adapters;

internal class SqlExceptionAdapterImplementation : SqlExceptionAdapter
{
	protected internal SqlException AdaptedSqlException => Exception as SqlException;

	protected internal override string Message => Exception.Message;

	protected internal override SqlExceptionAdapter InnerException => new SqlExceptionAdapterImplementation(Exception.InnerException);

	protected internal override SqlErrorCollectionAdapter Errors
	{
		get
		{
			if (AdaptedSqlException != null)
			{
				return new SqlErrorCollectionAdapterImplementation(AdaptedSqlException.Errors);
			}
			return null;
		}
	}

	protected internal override Guid ClientConnectionId
	{
		get
		{
			if (AdaptedSqlException != null)
			{
				return AdaptedSqlException.ClientConnectionId;
			}
			return Guid.Empty;
		}
	}

	protected internal override byte Class
	{
		get
		{
			if (AdaptedSqlException != null)
			{
				return AdaptedSqlException.Class;
			}
			return 0;
		}
	}

	protected internal override int LineNumber
	{
		get
		{
			if (AdaptedSqlException != null)
			{
				return AdaptedSqlException.LineNumber;
			}
			return 0;
		}
	}

	protected internal override int Number
	{
		get
		{
			if (AdaptedSqlException != null)
			{
				return AdaptedSqlException.Number;
			}
			return 0;
		}
	}

	protected internal override string Procedure
	{
		get
		{
			if (AdaptedSqlException != null)
			{
				return AdaptedSqlException.Procedure;
			}
			return string.Empty;
		}
	}

	protected internal override string Server
	{
		get
		{
			if (AdaptedSqlException != null)
			{
				return AdaptedSqlException.Server;
			}
			return string.Empty;
		}
	}

	protected internal override byte State
	{
		get
		{
			if (AdaptedSqlException != null)
			{
				return AdaptedSqlException.State;
			}
			return 0;
		}
	}

	protected internal SqlExceptionAdapterImplementation(Exception exception)
		: base(exception)
	{
	}
}
