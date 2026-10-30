using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSqlDatabaseLock : IDisposable
{
	private const string SqlDatabaseGlobalLockName = "GlobalLock";

	private const string SqlDatabasePerTenantLockPrefix = "PerTenantLock_";

	private const string ExclusiveLockMode = "Exclusive";

	private const string SharedLockMode = "Shared";

	private readonly object syncObject = new object();

	private Stack<string> acquiredLocks = new Stack<string>();

	private NavSqlConnectionScope lockOwnerConnectionScope;

	private NavSqlDatabaseLock()
	{
	}

	public static NavSqlDatabaseLock LockDatabase(NavSqlConnectionScope conScope, int timeout = 0)
	{
		if (conScope == null)
		{
			throw new ArgumentNullException("conScope");
		}
		NavSqlDatabaseLock navSqlDatabaseLock = new NavSqlDatabaseLock();
		if (Monitor.TryEnter(navSqlDatabaseLock.syncObject, timeout))
		{
			navSqlDatabaseLock.TryGetExclusiveSqlDatabaseLock(conScope, timeout);
		}
		return navSqlDatabaseLock;
	}

	public static NavSqlDatabaseLock LockTenant(NavSqlConnectionScope conScope, string tenantId, int timeout = 0)
	{
		if (conScope == null)
		{
			throw new ArgumentNullException("conScope");
		}
		if (tenantId == null)
		{
			throw new ArgumentNullException("tenantId");
		}
		NavSqlDatabaseLock navSqlDatabaseLock = new NavSqlDatabaseLock();
		if (Monitor.TryEnter(navSqlDatabaseLock.syncObject, timeout))
		{
			navSqlDatabaseLock.TryGetExclusiveTenantLock(conScope, tenantId, timeout);
		}
		return navSqlDatabaseLock;
	}

	public static NavSqlDatabaseLock LockResource(NavSqlConnectionScope conScope, string resourceId, int timeout = 0)
	{
		if (conScope == null)
		{
			throw new ArgumentNullException("conScope");
		}
		if (resourceId == null)
		{
			throw new ArgumentNullException("resourceId");
		}
		NavSqlDatabaseLock navSqlDatabaseLock = new NavSqlDatabaseLock();
		if (Monitor.TryEnter(navSqlDatabaseLock.syncObject, timeout))
		{
			navSqlDatabaseLock.TryGetSqlLock(conScope, resourceId, "Exclusive", timeout);
		}
		return navSqlDatabaseLock;
	}

	public bool IsLockAcquired()
	{
		return acquiredLocks.Count > 0;
	}

	private bool TryGetExclusiveSqlDatabaseLock(NavSqlConnectionScope conScope, int timeout = 0)
	{
		return TryGetSqlLock(conScope, "GlobalLock", "Exclusive", timeout);
	}

	private bool TryGetExclusiveTenantLock(NavSqlConnectionScope conScope, string tenantId, int timeout = 0)
	{
		if (TryGetSqlLock(conScope, "GlobalLock", "Shared", timeout))
		{
			if (!TryGetSqlLock(conScope, GetTenantLockName(tenantId), "Exclusive", timeout))
			{
				ReleaseSqlLock(conScope, "GlobalLock");
				acquiredLocks.Pop();
				return false;
			}
			return true;
		}
		return false;
	}

	private static string GetTenantLockName(string tenantId)
	{
		return "PerTenantLock_" + tenantId;
	}

	private bool TryGetSqlLock(NavSqlConnectionScope conScope, string lockName, string lockMode, int timeout)
	{
		if (timeout < 0)
		{
			timeout = int.MaxValue;
		}
		string commandText = string.Format(CultureInfo.InvariantCulture, "DECLARE @result int; EXEC @result = sp_getapplock @0, '{0}', @LockTimeout = {1}, @LockOwner = 'Session'; SELECT @result", lockMode, timeout);
		try
		{
			bool flag;
			using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection))
			{
				navSqlCommand.SetCommandText(commandText);
				navSqlCommand.Command.Parameters.Add(NavSqlStatementHelper.ParameterName(0), SqlDbType.VarChar, 250).Value = lockName;
				flag = navSqlCommand.ExecuteScalar<int>() >= 0;
			}
			if (flag)
			{
				if (conScope.Connection.Database.DatabaseType == NavDatabaseType.Tenant)
				{
					conScope.Connection.SetKeepAlive(enabled: true);
				}
				acquiredLocks.Push(lockName);
				lockOwnerConnectionScope = conScope;
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000087F", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Got sql lock '{0}', result = {1}", DiagnosticParameter.SystemMetadata(lockName), flag);
			return flag;
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000ZL", Category.Runtime, exception);
			return false;
		}
	}

	private static void ReleaseSqlLock(NavSqlConnectionScope conScope, string lockName)
	{
		try
		{
			int num;
			using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection))
			{
				navSqlCommand.SetCommandText("DECLARE @result int; EXEC @result = sp_releaseapplock @0, @LockOwner = 'Session'; SELECT @result");
				navSqlCommand.Command.Parameters.Add(NavSqlStatementHelper.ParameterName(0), SqlDbType.VarChar, 250).Value = lockName;
				num = navSqlCommand.ExecuteScalar<int>();
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000087G", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Release sql lock '{0}', result = {1}", DiagnosticParameter.SystemMetadata(lockName), num);
			if (num < 0)
			{
				throw new NavSqlDatabaseLockException(string.Format(CultureInfo.InvariantCulture, "Failed to release SQL database lock {0}.", lockName));
			}
		}
		catch (NavSqlException ex) when (ex.ErrorNumber == 1223)
		{
			throw new NavSqlDatabaseLockException("Database lock cannot be released because it is not hold by the current connection scope.", ex);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		try
		{
			while (acquiredLocks.Count > 0)
			{
				string text = acquiredLocks.Pop();
				try
				{
					ReleaseSqlLock(lockOwnerConnectionScope, text);
				}
				catch (Exception exception)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000087E", Category.Runtime, exception, Verbosity.Error, DiagnosticParameter.SystemMetadata(text));
				}
			}
		}
		finally
		{
			lockOwnerConnectionScope = null;
			Monitor.Exit(syncObject);
		}
	}
}
