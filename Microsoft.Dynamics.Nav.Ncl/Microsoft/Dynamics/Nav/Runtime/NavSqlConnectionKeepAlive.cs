using System;
using System.Data.SqlClient;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class NavSqlConnectionKeepAlive : IDisposable, INavSqlConnectionKeepAlive
{
	public delegate void KeepAliveEventHandler(object sender, EventArgs e);

	private const string KeepAliveSqlStatement = "SELECT GETDATE()";

	internal const int MaxNumberOfRetry = 5;

	private readonly NavSqlConnection connection;

	private readonly int keepAliveTime;

	private readonly Action<NavSqlConnection> keepAliveAction;

	private NonReEntrantTimerWrapper timer;

	private int numberOfFailure;

	public event KeepAliveEventHandler OnSuccess;

	public NavSqlConnectionKeepAlive(NavSqlConnection connection, int keepAliveTime)
		: this(connection, keepAliveTime, null)
	{
	}

	internal NavSqlConnectionKeepAlive(NavSqlConnection connection, int keepAliveTime, Action<NavSqlConnection> keepAliveAction)
	{
		if (connection == null)
		{
			throw new ArgumentNullException("connection");
		}
		if (keepAliveTime <= 0)
		{
			throw new ArgumentOutOfRangeException("keepAliveTime");
		}
		this.connection = connection;
		this.keepAliveTime = keepAliveTime;
		if (keepAliveAction == null)
		{
			keepAliveAction = (NavSqlConnection conn) =>
			{
				SqlCommand sqlCommand = new SqlCommand("SELECT GETDATE()", conn.Connection, conn.CurrentTransaction);
				sqlCommand.ExecuteNonQuery();
			};
		}
		this.keepAliveAction = keepAliveAction;
		timer = new NonReEntrantTimerWrapper("NavSqlConnectionKeepAlive", KeepAliveCallback, this, keepAliveTime);
	}

	public void Reset()
	{
		if (numberOfFailure < 5)
		{
			timer?.Change(keepAliveTime);
		}
	}

	public void Stop()
	{
		InterlockedAction.Dispose(ref timer);
	}

	private void KeepAliveCallback(object state)
	{
		try
		{
			if (timer == null || connection.IsDisposed || connection.Database.IsDisposed || connection.Database.Tenant.IsDisposed)
			{
				Stop();
			}
			else
			{
				if (keepAliveAction == null)
				{
					return;
				}
				if (connection.TryLock())
				{
					try
					{
						keepAliveAction(connection);
					}
					finally
					{
						connection.Unlock();
					}
				}
				numberOfFailure = 0;
				OnSuccess?.Invoke(this, EventArgs.Empty);
			}
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance(connection?.Database?.Tenant).SendExceptionTag("000015N", Category.SqlConnection, exception);
			numberOfFailure++;
			if (numberOfFailure >= 5)
			{
				DiagnosticsResolver.GetMostSpecificInstance(connection?.Database?.Tenant).SendTraceTag("000015O", Category.SqlConnection, Verbosity.Error, PrivacyClassification.SystemMetadata, "NavSqlConnection keep alive failed after {0} tries. No further keep alive request will be performed on this connection.", 5);
				Stop();
			}
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			Stop();
		}
	}
}
