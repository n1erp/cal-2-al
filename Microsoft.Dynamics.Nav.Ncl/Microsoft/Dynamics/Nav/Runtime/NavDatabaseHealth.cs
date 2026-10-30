using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavDatabaseHealth
{
	internal const string GetSystemDateCmd = "SELECT GETDATE()";

	private const int HealthCheckPeriod = 30;

	private const int HealthCheckMinimumInterval = 1;

	private readonly NavDatabase database;

	private readonly object healthSyncObj = new object();

	private bool isHealthy;

	private bool initialStateChecked;

	private long lastCheckTicks;

	public bool IsHealthy
	{
		get
		{
			CheckConnectionState();
			return isHealthy;
		}
	}

	public NavDatabaseHealth(NavDatabase database)
	{
		this.database = database;
	}

	private string CreateSqlEventMessage(string message)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Tenant ID: {0}", database.Tenant.Id));
		NavSession session = NavCurrentThread.Session;
		if (session != null)
		{
			stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Session type: {0}", session.ClientConnectionType));
			stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Session ID: {0}", session.Id));
		}
		stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, message, database.ServerName, database.DatabaseName));
		return stringBuilder.ToString();
	}

	private void CheckConnectionState()
	{
		long ticks = DateTime.UtcNow.Ticks;
		TimeSpan timeSpan = new TimeSpan(ticks - lastCheckTicks);
		lock (healthSyncObj)
		{
			if (isHealthy && timeSpan.TotalSeconds < 30.0)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000008N", Category.HealthCheck, Verbosity.Verbose, "Returning cached connection state for database '{0}' on server '{1}'", DiagnosticParameter.Oii(database.DatabaseName), DiagnosticParameter.SystemMetadata(database.DatabaseServer.ServerName));
				return;
			}
			if (timeSpan.TotalSeconds < 1.0)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000S6", Category.HealthCheck, Verbosity.Warning, "Health check is called too often. Returning cached connection state for database '{0}' on server '{1}'.", DiagnosticParameter.Oii(database.DatabaseName), DiagnosticParameter.SystemMetadata(database.DatabaseServer.ServerName));
				return;
			}
			lastCheckTicks = ticks;
			try
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000008O", Category.HealthCheck, Verbosity.Verbose, "Refreshing the connection state for database '{0}' on server '{1}'", DiagnosticParameter.Oii(database.DatabaseName), DiagnosticParameter.SystemMetadata(database.DatabaseServer.ServerName));
				using (SqlConnection sqlConnection = database.ConnectionInfo.CreateConnection())
				{
					sqlConnection.Open();
					using SqlCommand sqlCommand = sqlConnection.CreateCommand();
					sqlCommand.CommandText = "SELECT GETDATE()";
					sqlCommand.ExecuteScalar();
				}
				isHealthy = true;
			}
			catch (SqlException ex)
			{
				bool flag = NavSqlReturnCode.IsTransientError(ex.Number);
				if (ex.ErrorCode == 22924076)
				{
					ConnectionProblem("000008R", Category.HealthCheck, connectionPreviouslyOpen: false, flag);
				}
				else if (!flag)
				{
					isHealthy = false;
					throw;
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000008P", Category.HealthCheck, ex);
			}
		}
	}

	internal void ConnectionEstablished()
	{
		lock (healthSyncObj)
		{
			lastCheckTicks = DateTime.UtcNow.Ticks;
			if (!isHealthy || !initialStateChecked)
			{
				isHealthy = true;
				string message = CreateSqlEventMessage("A connection has been established\r\nDatabase server: {0}\r\nDatabase name: {1}.");
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000008Q", Category.HealthCheck, Verbosity.Verbose, PrivacyClassification.OrganizationIdentifiableInformation, message);
				NavServerEventSource.Log.WriteScomSqlConnectionEstablishedEvent(NavEnvironment.ServiceInstanceName, database.Tenant.Id, message);
				initialStateChecked = true;
			}
		}
	}

	internal void ConnectionProblem(string tag, Category category, bool connectionPreviouslyOpen, bool isTransientError)
	{
		lock (healthSyncObj)
		{
			if (!((!isHealthy && initialStateChecked) & isTransientError))
			{
				string message = CreateSqlEventMessage(connectionPreviouslyOpen ? "The connection to SQL server was lost\r\nDatabase server: {0}\r\nDatabase name: {1}." : "The connection to SQL server could not be established\r\nDatabase server: {0}\r\nDatabase name: {1}.");
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag(tag, category, isTransientError ? Verbosity.Warning : Verbosity.Error, PrivacyClassification.OrganizationIdentifiableInformation, message);
				if (!isTransientError)
				{
					NavServerEventSource.Log.WriteScomFatalSqlErrorEvent(NavEnvironment.ServiceInstanceName, database.Tenant.Id, message);
				}
				initialStateChecked = true;
				isHealthy = false;
			}
		}
	}
}
