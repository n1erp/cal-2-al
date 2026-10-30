using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Tracing;

namespace Microsoft.Dynamics.Nav.Types;

public class NavSqlDebuggingStatistics
{
	public const double TicksToMS = 10000.0;

	private int amountOfSqlStatementsInDebugger = ServerUserSettings.Instance.AmountOfSqlStatementsInDebugger.Value;

	private const int ExecuteScalar = 1;

	private const int ExecuteNonQuery = 2;

	private const int ExecuteReader = 3;

	private const int ReadNextRow = 5;

	private bool useDebuggingBehavior;

	private Queue<SqlStatementStatistics> sqlStatementsForDebuggingBehavior = new Queue<SqlStatementStatistics>();

	private Queue<SqlStatementStatistics> longRunningSqlStatementsForDebuggingBehavior = new Queue<SqlStatementStatistics>();

	private long globalSqlStatementsExecuted;

	private long globalSqlRowsRead;

	public bool SqlInformationDebuggerEnabled { get; }

	public bool EnableLongRunningSqlStatementsInDebugger { get; private set; }

	public int LongRunningSqlStatementsInDebuggerThreshold { get; private set; }

	public bool UseDebuggingBehavior
	{
		get
		{
			if (useDebuggingBehavior)
			{
				return SqlInformationDebuggerEnabled;
			}
			return false;
		}
	}

	public long GlobalSqlStatementsExecuted => globalSqlStatementsExecuted;

	public long GlobalSqlRowsRead => globalSqlRowsRead;

	public NavSqlDebuggingStatistics()
	{
		SqlInformationDebuggerEnabled = ServerUserSettings.Instance.EnableSqlInformationDebugger.Value;
		EnableLongRunningSqlStatementsInDebugger = ServerUserSettings.Instance.EnableLongRunningSqlStatementsInDebugger.Value;
		LongRunningSqlStatementsInDebuggerThreshold = ServerUserSettings.Instance.LongRunningSqlStatementsInDebuggerThreshold.Value;
		amountOfSqlStatementsInDebugger = ServerUserSettings.Instance.AmountOfSqlStatementsInDebugger.Value;
	}

	private void ResetDebuggingCache()
	{
		sqlStatementsForDebuggingBehavior = new Queue<SqlStatementStatistics>();
		longRunningSqlStatementsForDebuggingBehavior = new Queue<SqlStatementStatistics>();
	}

	public void StopDebuggingBehavior()
	{
		useDebuggingBehavior = false;
		ResetDebuggingCache();
	}

	public void StartDebuggingBehavior()
	{
		useDebuggingBehavior = true;
		ResetDebuggingCache();
	}

	public void StartDebuggingBehavior(int sqlStatementsInDebugger, bool enableLongRunningSqlStatementsInDebugger, int longRunningSqlStatementsInDebuggerThreshold)
	{
		amountOfSqlStatementsInDebugger = sqlStatementsInDebugger;
		EnableLongRunningSqlStatementsInDebugger = enableLongRunningSqlStatementsInDebugger;
		LongRunningSqlStatementsInDebuggerThreshold = longRunningSqlStatementsInDebuggerThreshold;
		useDebuggingBehavior = true;
		ResetDebuggingCache();
	}

	public void SqlFunctionExecuted(EventTask eventTask, string commandText, long durationTicks, Func<string> alCallStackAction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Expected I4, but got Unknown
		switch ((int)eventTask)
		{
		case 1:
		case 2:
		case 3:
		{
			globalSqlStatementsExecuted++;
			if (!useDebuggingBehavior)
			{
				break;
			}
			if (sqlStatementsForDebuggingBehavior.Count >= amountOfSqlStatementsInDebugger)
			{
				sqlStatementsForDebuggingBehavior.Dequeue();
			}
			DateTime executeStart = DateTime.UtcNow.AddTicks(-1 * durationTicks);
			SqlStatementStatistics sqlStatementStatistics = new SqlStatementStatistics
			{
				SqlStatement = commandText,
				ExecuteStart = executeStart,
				Duration = (double)durationTicks / 10000.0,
				SqlExecutionPlan = "<Maybe Implemented Later>",
				SqlIOStats = "<Maybe Implemented Later>"
			};
			sqlStatementsForDebuggingBehavior.Enqueue(sqlStatementStatistics);
			if (EnableLongRunningSqlStatementsInDebugger && sqlStatementStatistics.Duration >= (double)LongRunningSqlStatementsInDebuggerThreshold)
			{
				if (longRunningSqlStatementsForDebuggingBehavior.Count >= amountOfSqlStatementsInDebugger)
				{
					longRunningSqlStatementsForDebuggingBehavior.Dequeue();
				}
				sqlStatementStatistics = new SqlStatementStatistics
				{
					SqlStatement = commandText,
					ExecuteStart = executeStart,
					Duration = sqlStatementStatistics.Duration,
					SqlExecutionPlan = "<Maybe Implemented Later>",
					SqlIOStats = "<Maybe Implemented Later>",
					CallStack = alCallStackAction()
				};
				longRunningSqlStatementsForDebuggingBehavior.Enqueue(sqlStatementStatistics);
			}
			break;
		}
		case 5:
			globalSqlRowsRead++;
			if (!useDebuggingBehavior)
			{
				break;
			}
			{
				foreach (SqlStatementStatistics item in sqlStatementsForDebuggingBehavior.Reverse())
				{
					if (string.Equals(commandText, item.SqlStatement))
					{
						item.RowsRead++;
						break;
					}
				}
				break;
			}
		}
	}

	public SqlStatementStatistics[] GetDebuggingExecutedSqlStatements()
	{
		return sqlStatementsForDebuggingBehavior.ToArray();
	}

	internal SqlStatementStatistics[] GetDebuggingLongRunningSqlStatements()
	{
		return longRunningSqlStatementsForDebuggingBehavior.ToArray();
	}
}
