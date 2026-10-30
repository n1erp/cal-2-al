using System;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class BufferTenantManagement
{
	public static void RegisterBufferTenant(NavDatabase database, string registeredTenantId, string tenantId, bool ignoreDataVersion)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Update);
		string tenantIdParameter = NavSqlStatementHelper.ParameterName(0);
		string registeredTenantIdParameter = NavSqlStatementHelper.ParameterName(1);
		string applicationVersionParameter = NavSqlStatementHelper.ParameterName(2);
		string ignoreDataVersionParameter = NavSqlStatementHelper.ParameterName(3);
		string idField = NavSqlTenantProperties.FieldNames[0];
		string typeField = NavSqlTenantProperties.FieldNames[6];
		string versionField = NavSqlTenantProperties.FieldNames[5];
		string appInsightIdsField = NavSqlTenantProperties.FieldNames[8];
		string text = navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("LOCKBUFFERTENANTROW", database), () => FormattableString.Invariant(FormattableStringFactory.Create("\r\n                           SELECT TOP 1 [{0}]\r\n                             FROM {1} WITH(UPDLOCK)\r\n                            WHERE [{2}] = {3}\r\n                              AND [{4}] = {5}", versionField, "[dbo].[$ndo$tenantproperty]", typeField, 3, idField, tenantIdParameter)), (NavSqlCommand commandToExecute) =>
		{
			SqlParameter value = NavSqlStatementHelper.CreateSqlParameter(tenantIdParameter, NavSqlTenantProperties.FieldTypes[0], SqlDataType.Default, NavSqlTenantProperties.FieldLengths[0], tenantId, isWildcardValue: false, isFilterValue: false);
			commandToExecute.Command.Parameters.Add(value);
		}, (NavSqlCommand commandToExecute) =>
		{
			commandToExecute.Command.Parameters[0].Value = tenantId;
		}, (NavSqlCommand commandToExecute) => commandToExecute.ExecuteScalar<string>());
		if (text == null)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000TE", Category.Tenant, NavBufferTenantNotFoundException.Create(tenantId, database.Tenant.TenantSettings.TenantDatabaseId));
		}
		string applicationVersion = NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion;
		if (!ignoreDataVersion && text != applicationVersion)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001VM", Category.Tenant, NavBufferTenantDataVersionMismatchException.CreateForSpecificTenant(tenantId, database.Tenant.TenantSettings.TenantDatabaseId, text, applicationVersion));
		}
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("REGISTERBUFFERTENANTWITHID", database), () => FormattableString.Invariant(FormattableStringFactory.Create("\r\n                            UPDATE {0}\r\n                               SET [{1}] = {2}\r\n                                 , [{3}] = {4}\r\n                                 , [{5}] = NULL\r\n                             WHERE [{6}] = {7}\r\n                               AND [{8}] = {9}\r\n                               AND (\r\n                                       1 = {10}\r\n                                    OR [{11}] = {12}\r\n                                   )\r\n                             SELECT @@ROWCOUNT", "[dbo].[$ndo$tenantproperty]", typeField, 0, idField, registeredTenantIdParameter, appInsightIdsField, idField, tenantIdParameter, typeField, 3, ignoreDataVersionParameter, versionField, applicationVersionParameter)), (NavSqlCommand commandToExecute) =>
		{
			SqlParameter value = NavSqlStatementHelper.CreateSqlParameter(tenantIdParameter, NavSqlTenantProperties.FieldTypes[0], SqlDataType.Default, NavSqlTenantProperties.FieldLengths[0], tenantId, isWildcardValue: false, isFilterValue: false);
			commandToExecute.Command.Parameters.Add(value);
			value = NavSqlStatementHelper.CreateSqlParameter(registeredTenantIdParameter, NavSqlTenantProperties.FieldTypes[0], SqlDataType.Default, NavSqlTenantProperties.FieldLengths[0], registeredTenantId, isWildcardValue: false, isFilterValue: false);
			commandToExecute.Command.Parameters.Add(value);
			value = NavSqlStatementHelper.CreateSqlParameter(applicationVersionParameter, NavSqlTenantProperties.FieldTypes[5], SqlDataType.Default, NavSqlTenantProperties.FieldLengths[5], database.TenantDatabaseVersion.GetHighestSupportedApplicationVersion().ToString(), isWildcardValue: false, isFilterValue: false);
			commandToExecute.Command.Parameters.Add(value);
			value = NavSqlStatementHelper.CreateSqlParameter(ignoreDataVersionParameter, NavNclType.NavBoolean, SqlDataType.Default, 0, ignoreDataVersion, isWildcardValue: false, isFilterValue: false);
			commandToExecute.Command.Parameters.Add(value);
		}, (NavSqlCommand commandToExecute) =>
		{
			commandToExecute.Command.Parameters[0].Value = tenantId;
			commandToExecute.Command.Parameters[1].Value = registeredTenantId;
			commandToExecute.Command.Parameters[2].Value = database.TenantDatabaseVersion.GetHighestSupportedApplicationVersion().ToString();
			commandToExecute.Command.Parameters[3].Value = ignoreDataVersion;
		}, (NavSqlCommand commandToExecute) => ExecuteRegisterBufferTenantCommand(database, registeredTenantId, commandToExecute));
	}

	public static bool RegisterBufferTenant(NavDatabase database, string registeredTenantId, string applicationVersion)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Update);
		string registeredTenantIdParameter = NavSqlStatementHelper.ParameterName(0);
		string applicationVersionParameter = NavSqlStatementHelper.ParameterName(1);
		return navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("REGISTERBUFFERTENANT", database), () =>
		{
			string text = "@tenantId";
			StringBuilder stringBuilder = new StringBuilder(250);
			stringBuilder.AppendLine(FormattableString.Invariant($"DECLARE {text} NVARCHAR(128)"));
			string text2 = NavSqlTenantProperties.FieldNames[0];
			string text3 = NavSqlTenantProperties.FieldNames[6];
			string text4 = NavSqlTenantProperties.FieldNames[5];
			stringBuilder.AppendLine(FormattableString.Invariant(FormattableStringFactory.Create("\r\n                            SELECT TOP 1 {0} = [{1}]\r\n                             FROM {2} WITH(UPDLOCK, READPAST)\r\n                            WHERE [{3}] = {4}\r\n                              AND [{5}] = {6}", text, text2, "[dbo].[$ndo$tenantproperty]", text3, 3, text4, applicationVersionParameter)));
			stringBuilder.AppendLine(FormattableString.Invariant(FormattableStringFactory.Create("\r\n                            UPDATE {0}\r\n                               SET [{1}] = {2}\r\n                                 , [{3}] = {4}\r\n                             WHERE [{5}] = {6}\r\n                             SELECT @@ROWCOUNT", "[dbo].[$ndo$tenantproperty]", text3, 0, text2, registeredTenantIdParameter, text2, text)));
			return stringBuilder.ToString();
		}, (NavSqlCommand command) =>
		{
			SqlParameter value = NavSqlStatementHelper.CreateSqlParameter(registeredTenantIdParameter, NavSqlTenantProperties.FieldTypes[0], SqlDataType.Default, NavSqlTenantProperties.FieldLengths[0], registeredTenantId, isWildcardValue: false, isFilterValue: false);
			command.Command.Parameters.Add(value);
			value = NavSqlStatementHelper.CreateSqlParameter(applicationVersionParameter, NavSqlTenantProperties.FieldTypes[5], SqlDataType.Default, NavSqlTenantProperties.FieldLengths[5], applicationVersion, isWildcardValue: false, isFilterValue: false);
			command.Command.Parameters.Add(value);
		}, (NavSqlCommand command) =>
		{
			command.Command.Parameters[0].Value = registeredTenantId;
			command.Command.Parameters[1].Value = applicationVersion;
		}, (NavSqlCommand command) => ExecuteRegisterBufferTenantCommand(database, registeredTenantId, command));
	}

	private static bool ExecuteRegisterBufferTenantCommand(NavDatabase database, string registeredTenantId, NavSqlCommand command)
	{
		try
		{
			int num = command.ExecuteScalar<int>();
			bool flag = num > 0;
			if (flag)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000TF", Category.Tenant, Verbosity.Normal, "Tenant {0} registered in database {1}", DiagnosticParameter.Oii(registeredTenantId), DiagnosticParameter.Oii(database.DatabaseName));
			}
			return flag;
		}
		catch (NavCSideDuplicateKeyException)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000TG", Category.Tenant, NavDuplicateTenantIdException.CreateForDuplicateInDatabase(registeredTenantId, database.DatabaseName));
		}
	}

	public static int GetNumberOfBufferTenants(string databaseName, string databaseServerName, string databaseUserName, ProtectedDatabasePassword password, bool includeObsoleteBufferTenants)
	{
		using NavTenant navTenant = CreateTemporaryTenant(databaseName, databaseServerName, databaseUserName, password);
		return GetNumberOfBufferTenants(navTenant.Database, includeObsoleteBufferTenants);
	}

	public static int GetNumberOfBufferTenants(NavDatabase database, bool includeObsoleteBufferTenants)
	{
		string applicationVersion = NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion;
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Browse);
		string applicationVersionParameter = NavSqlStatementHelper.ParameterName(0);
		string includeObsoleteBufferTenantsParameter = NavSqlStatementHelper.ParameterName(1);
		return navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("GETNUMBEROFBUFFERTENANTS", database), () =>
		{
			string text = NavSqlTenantProperties.FieldNames[6];
			string text2 = NavSqlTenantProperties.FieldNames[5];
			return FormattableString.Invariant(FormattableStringFactory.Create("SELECT COUNT(*) FROM {0} WHERE [{1}] = {2} AND\r\n                            ([{3}] = {4} OR 1 = {5})", "[dbo].[$ndo$tenantproperty]", text, 3, text2, applicationVersionParameter, includeObsoleteBufferTenantsParameter));
		}, (NavSqlCommand command) =>
		{
			SqlParameter value = NavSqlStatementHelper.CreateSqlParameter(applicationVersionParameter, NavSqlTenantProperties.FieldTypes[5], SqlDataType.Default, NavSqlTenantProperties.FieldLengths[5], applicationVersion, isWildcardValue: false, isFilterValue: false);
			command.Command.Parameters.Add(value);
			value = NavSqlStatementHelper.CreateSqlParameter(includeObsoleteBufferTenantsParameter, NavNclType.NavBoolean, SqlDataType.Default, 0, includeObsoleteBufferTenants, isWildcardValue: false, isFilterValue: false);
			command.Command.Parameters.Add(value);
		}, (NavSqlCommand command) =>
		{
			command.Command.Parameters[0].Value = applicationVersion;
			command.Command.Parameters[1].Value = includeObsoleteBufferTenants;
		}, (NavSqlCommand command) => command.ExecuteScalar<int>());
	}

	private static NavTenant CreateTemporaryTenant(string databaseName, string databaseServerName, string databaseUserName, ProtectedDatabasePassword password)
	{
		NavDatabaseServer orAddDatabaseServerByName = NavEnvironment.Instance.DatabaseServers.GetOrAddDatabaseServerByName(databaseServerName, databaseUserName, password);
		ExternalSqlTableConnectionSettings externalSqlTableConnectionSettings = new ExternalSqlTableConnectionSettings(databaseName, orAddDatabaseServerByName.ServerName, databaseName, orAddDatabaseServerByName.UserName, orAddDatabaseServerByName.Password, ServerUserSettings.Instance.SqlConnectionTimeout.TotalSeconds);
		return new NavExternalTenant(orAddDatabaseServerByName, databaseName, externalSqlTableConnectionSettings);
	}
}
