using System;
using System.Data.Common;

namespace Microsoft.Dynamics.Nav.Runtime.Sql.Adapters;

internal abstract class SqlExceptionAdapter
{
	internal Exception Exception { get; private set; }

	protected internal DbException DatabaseException => Exception as DbException;

	protected internal abstract string Message { get; }

	protected internal abstract SqlExceptionAdapter InnerException { get; }

	protected internal abstract SqlErrorCollectionAdapter Errors { get; }

	protected internal abstract Guid ClientConnectionId { get; }

	protected internal abstract byte Class { get; }

	protected internal abstract int LineNumber { get; }

	protected internal abstract int Number { get; }

	protected internal abstract string Procedure { get; }

	protected internal abstract string Server { get; }

	protected internal abstract byte State { get; }

	protected internal SqlExceptionAdapter(Exception exception)
	{
		Exception = exception;
	}
}
