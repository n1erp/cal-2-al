using System;
using System.Text;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

public class SqlProfilerCommentProvider
{
	private enum GetConnectionState
	{
		NotSet,
		GetConnectionFromThePool,
		ExecSqlStatement,
		ReturnConnectionToThePool
	}

	internal const string StartComment = "/*";

	internal const string EndComment = "*/";

	internal const string User = "User: ";

	internal const string GetConnectionFromThePool = "Get connection from the pool.";

	internal const string ReturnConnectionToThePool = "Return connection to the pool.";

	private static int numberOfEnabledSession;

	private GetConnectionState getConnectionState;

	private NavSession owningNavSession;

	private IExecutionListener sqlProfilerCommentProviderExecutionListener;

	private readonly object syncObject = new object();

	private bool isEnabled;

	internal static bool IsEnabledForNewConnections { get; set; }

	internal static bool IsEnabledForAnySession => numberOfEnabledSession > 0;

	internal bool AlStatementExecutedOrMethodExited { get; set; }

	internal bool IsEnabled
	{
		get
		{
			return isEnabled;
		}
		set
		{
			lock (syncObject)
			{
				if (isEnabled != value)
				{
					isEnabled = value;
					if (!isEnabled)
					{
						DeleteExecutionListener();
						getConnectionState = GetConnectionState.NotSet;
						Interlocked.Decrement(ref numberOfEnabledSession);
					}
					else
					{
						AlStatementExecutedOrMethodExited = true;
						Interlocked.Increment(ref numberOfEnabledSession);
					}
				}
			}
		}
	}

	private static ALCodeEnvironment CodeEnvironment => NavGlobal.NCLMetadata.CodeEnvironment;

	internal void Initialize(NavSession navSession)
	{
		isEnabled = IsEnabledForNewConnections;
		if (isEnabled)
		{
			Interlocked.Increment(ref numberOfEnabledSession);
		}
		owningNavSession = navSession;
		AlStatementExecutedOrMethodExited = true;
		getConnectionState = GetConnectionState.NotSet;
	}

	internal void WriteAlCallStackToSql(NavDatabase database)
	{
		if (isEnabled && AlStatementExecutedOrMethodExited)
		{
			EnsureExecutionListenerIsCreated();
			AlStatementExecutedOrMethodExited = false;
			BeforeSqlStatementExecuted(database);
			StringBuilder stringBuilder = new StringBuilder(100);
			if (owningNavSession.CurrentMethodScope != null && !(owningNavSession.CurrentMethodScope is ApplicationObjectRootScope) && owningNavSession.CurrentMethodScope.ParentScope != null)
			{
				stringBuilder.Append(GetCurrentStatement());
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.Append(owningNavSession.GetCurrentCallstack(GetObjectName));
			}
			WriteStringToSqlProfilerUserNameAdded(stringBuilder.ToString(), database);
		}
	}

	internal void WriteGetOrReturnConnectionToThePoolToSqlProfiler(int oldInUse, int newInUse, NavDatabase database)
	{
		if (!isEnabled)
		{
			return;
		}
		if (newInUse == 0)
		{
			if (getConnectionState == GetConnectionState.ExecSqlStatement)
			{
				WriteStringToSqlProfilerUserNameAdded("Return connection to the pool." + Environment.NewLine, database);
			}
			getConnectionState = GetConnectionState.ReturnConnectionToThePool;
		}
		else if (oldInUse == 0 && newInUse == 1)
		{
			getConnectionState = GetConnectionState.GetConnectionFromThePool;
		}
	}

	internal void BeforeSqlStatementExecuted(NavSqlCommand command)
	{
		if (isEnabled)
		{
			BeforeSqlStatementExecuted(command.Connection.Database);
		}
	}

	internal void BeforeSqlStatementExecuted(NavDatabase database)
	{
		if (isEnabled)
		{
			if (getConnectionState == GetConnectionState.GetConnectionFromThePool)
			{
				WriteStringToSqlProfilerUserNameAdded("Get connection from the pool." + Environment.NewLine, database);
			}
			getConnectionState = GetConnectionState.ExecSqlStatement;
		}
	}

	protected virtual void WriteStringToSqlProfiler(string comment, NavDatabase database)
	{
		if (!isEnabled)
		{
			return;
		}
		isEnabled = false;
		try
		{
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database);
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			navSqlCommand.ExecuteNonQueryNoTrace(comment);
		}
		finally
		{
			isEnabled = true;
		}
	}

	private void WriteStringToSqlProfilerUserNameAdded(string comment, NavDatabase database)
	{
		StringBuilder stringBuilder = new StringBuilder("/*", 100).Append(Environment.NewLine);
		stringBuilder.Append(comment);
		stringBuilder.Append("User: ").Append(owningNavSession.User.Name);
		stringBuilder.Append(Environment.NewLine);
		stringBuilder.Append("*/");
		WriteStringToSqlProfiler(stringBuilder.ToString(), database);
	}

	private void DeleteExecutionListener()
	{
		ExecutionListener.RemoveListener(sqlProfilerCommentProviderExecutionListener);
		sqlProfilerCommentProviderExecutionListener = null;
	}

	private void EnsureExecutionListenerIsCreated()
	{
		if (sqlProfilerCommentProviderExecutionListener == null)
		{
			sqlProfilerCommentProviderExecutionListener = new SqlProfilerCommentProviderExecutionListener(owningNavSession);
			ExecutionListener.AddListener(sqlProfilerCommentProviderExecutionListener);
		}
	}

	private static string GetObjectName(NavApplicationObjectBase applicationObject)
	{
		return CodeEnvironment.GetSourceCodeInfo(applicationObject.ObjectId).Name;
	}

	private string GetCurrentStatement()
	{
		NavMethodScope currentMethodScope = owningNavSession.CurrentMethodScope;
		return CodeEnvironment.GetLineNumberAndStatement(currentMethodScope).Item2;
	}
}
