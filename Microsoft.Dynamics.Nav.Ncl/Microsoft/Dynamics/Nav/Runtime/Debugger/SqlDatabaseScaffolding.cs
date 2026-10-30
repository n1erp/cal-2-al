using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class SqlDatabaseScaffolding : Scaffolding
{
	public override string TypeString => string.Empty;

	protected override bool ProbablyHasChildren => true;

	public SqlDatabaseScaffolding()
		: base("<Database Statistics>", typeof(NavSession), new IdentityValueBinder(), childrenTypesCanChange: false, promotedChildren: true)
	{
	}

	protected internal override string GetStringValue(object instance)
	{
		return string.Empty;
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (instance != null)
		{
			return instance.GetType() == TargetType;
		}
		return false;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(object instance)
	{
		NavSession session = instance as NavSession;
		long sqlLatency = GetSqlLatencyTicks(session);
		if (sqlLatency != -1)
		{
			yield return new TypeScaffolding("Current SQL Latency (ms)", typeof(double), new DelegateValueBinder((object input) => (double)sqlLatency / 10000.0, typeof(double)));
		}
		yield return new TypeScaffolding("Number of SQL Executes", typeof(long), new DelegateValueBinder((object input) => session.SqlDebuggingStatistics.GlobalSqlStatementsExecuted, typeof(long)));
		yield return new TypeScaffolding("Number of SQL Row Reads", typeof(long), new DelegateValueBinder((object input) => session.SqlDebuggingStatistics.GlobalSqlRowsRead, typeof(long)));
		SqlStatementStatistics[] debuggingExecutedSqlStatements = session.SqlDebuggingStatistics.GetDebuggingExecutedSqlStatements();
		SqlStatementListScaffolding sqlStatementListScaffolding = AddStatementListScaffoldings("<Last Executed SQL Statements>", debuggingExecutedSqlStatements);
		if (sqlStatementListScaffolding != null)
		{
			yield return sqlStatementListScaffolding;
		}
		debuggingExecutedSqlStatements = session.SqlDebuggingStatistics.GetDebuggingLongRunningSqlStatements();
		SqlStatementListScaffolding sqlStatementListScaffolding2 = AddStatementListScaffoldings("<Last Long Running SQL Statements>", debuggingExecutedSqlStatements);
		if (sqlStatementListScaffolding2 != null)
		{
			yield return sqlStatementListScaffolding2;
		}
	}

	private static SqlStatementListScaffolding AddStatementListScaffoldings(string nodeName, SqlStatementStatistics[] statements)
	{
		if (statements.Length != 0)
		{
			SqlStatementListScaffolding sqlStatementListScaffolding = new SqlStatementListScaffolding(nodeName);
			sqlStatementListScaffolding.Expand(statements);
			if (sqlStatementListScaffolding.Children.Count > 0)
			{
				return sqlStatementListScaffolding;
			}
		}
		return null;
	}

	internal static long GetSqlLatencyTicks(NavSession session)
	{
		try
		{
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(session.Database);
			using SqlCommand sqlCommand = navSqlConnectionScope.Connection.Connection.CreateCommand();
			Stopwatch stopwatch = Stopwatch.StartNew();
			sqlCommand.Transaction = navSqlConnectionScope.Connection.CurrentTransaction;
			sqlCommand.CommandText = "SELECT GETDATE()";
			sqlCommand.ExecuteScalar();
			stopwatch.Stop();
			return stopwatch.ElapsedTicks;
		}
		catch (Exception exception)
		{
			session.Diagnostics.SendExceptionTag("0000810", Category.Development, exception);
			return -1L;
		}
	}
}
