using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlTenantDatabaseVersion
{
	public const string TableName = "[dbo].[$ndo$tenantdatabaseversion]";

	public const string ApplicationVersionFieldName = "applicationversion";

	public const string ApplicationFamilyFieldName = "applicationfamily";

	private string ApplicationVersionParamName = "@0";

	private string ApplicationFamilyParamName = "@1";

	private string UpdateAppVersionParamName = "@2";

	internal const string PrimaryKeyName = "PK$ndo$tenantdatabaseversion";

	internal static readonly string[] FieldNames = new string[2] { "applicationversion", "applicationfamily" };

	internal static readonly NavNclType[] FieldTypes = new NavNclType[2]
	{
		NavNclType.NavText,
		NavNclType.NavText
	};

	internal static readonly SqlDbType[] FieldSqlTypes = new SqlDbType[2]
	{
		SqlDbType.NVarChar,
		SqlDbType.NVarChar
	};

	internal static readonly int[] FieldLengths = new int[2] { 50, 50 };

	private const int ApplicationVersionFieldNo = 0;

	private const int ApplicationFamilyFieldNo = 1;

	private readonly NavDatabase tenantDatabase;

	private readonly object lockObj = new object();

	private bool isReady;

	private bool isTableExists;

	private string applicationVersion;

	private string applicationFamily;

	public string ApplicationVersion
	{
		get
		{
			Read();
			return applicationVersion;
		}
	}

	public string ApplicationFamily
	{
		get
		{
			Read();
			return applicationFamily;
		}
	}

	public bool TableExists
	{
		get
		{
			Read();
			return isTableExists;
		}
	}

	internal static StringBuilder SelectTenantDatabaseApplicationVersionStatement
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("SELECT TOP 1 ").Append("applicationversion").Append(" FROM ")
				.Append("[dbo].[$ndo$tenantdatabaseversion]");
			return stringBuilder;
		}
	}

	internal static StringBuilder SelectAllSupportedTenantDatabaseApplicationVersionStatement
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("SELECT DISTINCT ").Append("applicationversion").Append(" FROM ")
				.Append("[dbo].[$ndo$tenantdatabaseversion]");
			return stringBuilder;
		}
	}

	internal static StringBuilder SelectTenantDatabaseApplicationFamilyStatement
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("SELECT TOP 1 ").Append("applicationfamily").Append(" FROM ")
				.Append("[dbo].[$ndo$tenantdatabaseversion]");
			return stringBuilder;
		}
	}

	internal static StringBuilder UpdateTenantDatabaseApplicationVersionStatement
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("UPDATE ").Append("[dbo].[$ndo$tenantdatabaseversion]");
			return stringBuilder;
		}
	}

	internal static StringBuilder DeleteTenantDatabaseApplicationVersionStatement
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("DELETE FROM ").Append("[dbo].[$ndo$tenantdatabaseversion]");
			return stringBuilder;
		}
	}

	internal static StringBuilder CountTenantDatabaseSupportedApplicationVersionStatement
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("SELECT COUNT(*) FROM ").Append("[dbo].[$ndo$tenantdatabaseversion]");
			return stringBuilder;
		}
	}

	internal static StringBuilder TruncateApplicationVersionTableStatement
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("TRUNCATE TABLE ").Append("[dbo].[$ndo$tenantdatabaseversion]");
			return stringBuilder;
		}
	}

	internal static StringBuilder GetTenantDatabaseVersionsAndFamiliesStatement
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("SELECT ").Append("applicationversion").Append(", ")
				.Append("applicationfamily")
				.Append(" FROM ")
				.Append("[dbo].[$ndo$tenantdatabaseversion]")
				.Append(" ");
			return stringBuilder;
		}
	}

	internal static string InsertTenantDatabaseVersionStatement
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("INSERT INTO ").Append("[dbo].[$ndo$tenantdatabaseversion]").Append(" VALUES(@0,@1)");
			return stringBuilder.ToString();
		}
	}

	public NavSqlTenantDatabaseVersion(NavDatabase tenantDatabase)
	{
		if (tenantDatabase.DatabaseType != NavDatabaseType.Tenant)
		{
			throw new ArgumentException("The given database is not a tenant database.");
		}
		this.tenantDatabase = tenantDatabase;
	}

	public void StoreApplicationVersionSetting(string newApplicationVersion)
	{
		ThrowInvalidOperationExceptionIfDatabaseNotUpToDate();
		lock (lockObj)
		{
			Read();
			applicationVersion = newApplicationVersion;
			WriteFieldToDatabase(0);
		}
	}

	public void StoreApplicationFamilySetting(string newApplicationFamily)
	{
		ThrowInvalidOperationExceptionIfDatabaseNotUpToDate();
		lock (lockObj)
		{
			Read();
			applicationFamily = newApplicationFamily;
			WriteFieldToDatabase(1);
		}
	}

	public void Refresh()
	{
		lock (lockObj)
		{
			isReady = false;
			Read();
		}
	}

	public void InsertNewApplicationVersion(string newAppVersion, string newAppFamily)
	{
		if (string.IsNullOrEmpty(newAppVersion) || newAppFamily == null)
		{
			return;
		}
		string text = ReadFirstApplicationFamily();
		if (!string.IsNullOrEmpty(text) && !text.Equals(newAppFamily))
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The application family ({0}) of the tenant database is not compatible with the current application family ({1}) of the application database on the {2} instance.", tenantDatabase.Tenant.Id, tenantDatabase.ServerName, ProductNames.ProductNameServer));
		}
		lock (lockObj)
		{
			using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true))
			{
				StringBuilder stringBuilder = new StringBuilder(InsertTenantDatabaseVersionStatement);
				using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
				navSqlCommand.Command.Parameters.Add(new SqlParameter(ApplicationVersionParamName, FieldSqlTypes[0], FieldLengths[0]));
				navSqlCommand.Command.Parameters.Add(new SqlParameter(ApplicationFamilyParamName, FieldSqlTypes[1], FieldLengths[1]));
				navSqlCommand.Command.Parameters[0].Value = newAppVersion;
				navSqlCommand.Command.Parameters[1].Value = newAppFamily;
				navSqlCommand.ExecuteNonQuery(stringBuilder.ToString());
			}
			Refresh();
		}
	}

	public bool CheckIfSupportsApplicationVersion(string checkedAppVersion, string checkedAppFamily = null)
	{
		if (string.IsNullOrEmpty(checkedAppVersion))
		{
			return false;
		}
		lock (lockObj)
		{
			Refresh();
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true);
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			StringBuilder selectTenantDatabaseApplicationVersionStatement = SelectTenantDatabaseApplicationVersionStatement;
			selectTenantDatabaseApplicationVersionStatement.Append(" WHERE (");
			selectTenantDatabaseApplicationVersionStatement.Append(FieldNames[0]);
			selectTenantDatabaseApplicationVersionStatement.Append('=');
			selectTenantDatabaseApplicationVersionStatement.Append(ApplicationVersionParamName);
			navSqlCommand.Command.Parameters.Add(new SqlParameter(ApplicationVersionParamName, FieldSqlTypes[0], FieldLengths[0]));
			navSqlCommand.Command.Parameters[ApplicationVersionParamName].Value = checkedAppVersion;
			if (checkedAppFamily != null)
			{
				selectTenantDatabaseApplicationVersionStatement.Append(" and ");
				selectTenantDatabaseApplicationVersionStatement.Append(FieldNames[1]);
				selectTenantDatabaseApplicationVersionStatement.Append('=');
				selectTenantDatabaseApplicationVersionStatement.Append(ApplicationFamilyParamName);
				navSqlCommand.Command.Parameters.Add(new SqlParameter(ApplicationFamilyParamName, FieldSqlTypes[1], FieldLengths[1]));
				navSqlCommand.Command.Parameters[ApplicationFamilyParamName].Value = checkedAppFamily;
			}
			selectTenantDatabaseApplicationVersionStatement.Append(')');
			object obj = navSqlCommand.ExecuteScalar(selectTenantDatabaseApplicationVersionStatement.ToString());
			if (obj != null)
			{
				return true;
			}
			return false;
		}
	}

	public string GetNextSupportedApplicationVersion(string fromAppVersionStr, string checkedAppFamily = null)
	{
		if (string.IsNullOrEmpty(fromAppVersionStr))
		{
			return null;
		}
		Version version = Version.Parse(fromAppVersionStr);
		Version version2 = null;
		lock (lockObj)
		{
			Refresh();
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true);
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			StringBuilder selectAllSupportedTenantDatabaseApplicationVersionStatement = SelectAllSupportedTenantDatabaseApplicationVersionStatement;
			if (checkedAppFamily != null)
			{
				AppendAppFamilyCondition(checkedAppFamily, selectAllSupportedTenantDatabaseApplicationVersionStatement, navSqlCommand);
			}
			using (navSqlCommand.ExecuteReader(selectAllSupportedTenantDatabaseApplicationVersionStatement.ToString()))
			{
				while (navSqlCommand.ReadNextRow())
				{
					string input = (navSqlCommand.Reader.IsDBNull(0) ? string.Empty : navSqlCommand.Reader.GetString(0));
					if (Version.TryParse(input, out var result) && !(result <= version))
					{
						if (version2 == null)
						{
							version2 = result;
						}
						else if (result < version2)
						{
							version2 = result;
						}
					}
				}
			}
		}
		return version2?.ToString();
	}

	public string GetHighestSupportedApplicationVersion(string checkedAppFamily = null)
	{
		Version version = null;
		lock (lockObj)
		{
			Refresh();
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true);
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			StringBuilder selectAllSupportedTenantDatabaseApplicationVersionStatement = SelectAllSupportedTenantDatabaseApplicationVersionStatement;
			if (checkedAppFamily != null)
			{
				AppendAppFamilyCondition(checkedAppFamily, selectAllSupportedTenantDatabaseApplicationVersionStatement, navSqlCommand);
			}
			using (navSqlCommand.ExecuteReader(selectAllSupportedTenantDatabaseApplicationVersionStatement.ToString()))
			{
				while (navSqlCommand.ReadNextRow())
				{
					string input = (navSqlCommand.Reader.IsDBNull(0) ? string.Empty : navSqlCommand.Reader.GetString(0));
					if (Version.TryParse(input, out var result))
					{
						version = ((result < version) ? version : result);
					}
				}
			}
		}
		return version?.ToString();
	}

	private void AppendAppFamilyCondition(string checkedAppFamily, StringBuilder commandText, NavSqlCommand command)
	{
		commandText.Append(" WHERE (");
		commandText.Append(FieldNames[1]);
		commandText.Append('=');
		commandText.Append(ApplicationFamilyParamName);
		command.Command.Parameters.Add(new SqlParameter(ApplicationFamilyParamName, FieldSqlTypes[1], FieldLengths[1]));
		command.Command.Parameters[ApplicationFamilyParamName].Value = checkedAppFamily;
		commandText.Append(')');
	}

	public string ReadFirstApplicationFamily()
	{
		lock (lockObj)
		{
			Refresh();
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true);
			StringBuilder selectTenantDatabaseApplicationFamilyStatement = SelectTenantDatabaseApplicationFamilyStatement;
			selectTenantDatabaseApplicationFamilyStatement.Append(" WHERE (");
			selectTenantDatabaseApplicationFamilyStatement.Append("applicationfamily");
			selectTenantDatabaseApplicationFamilyStatement.Append(' ');
			selectTenantDatabaseApplicationFamilyStatement.Append("IS NOT NULL");
			selectTenantDatabaseApplicationFamilyStatement.Append(" and ");
			selectTenantDatabaseApplicationFamilyStatement.Append("applicationfamily");
			selectTenantDatabaseApplicationFamilyStatement.Append("<>''");
			selectTenantDatabaseApplicationFamilyStatement.Append(')');
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			return navSqlCommand.ExecuteScalar(selectTenantDatabaseApplicationFamilyStatement.ToString())?.ToString();
		}
	}

	public void UpdateTenantDatabaseVersion(string updatedApplicationVersion, string newAppFamily, string newApplicationVersion = null)
	{
		if (string.IsNullOrEmpty(updatedApplicationVersion))
		{
			throw new InvalidOperationException("The application version field of the application database cannot be empty. To set the application version, use the ApplicationVersion parameter of the Set-NAVApplication cmdlet.");
		}
		string text = ReadFirstApplicationFamily();
		if (!string.IsNullOrEmpty(text) && !text.Equals(newAppFamily))
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The application family ({0}) of the tenant database is not compatible with the current application family ({1}) of the application database on the {2} instance.", tenantDatabase.Tenant.Id, tenantDatabase.ServerName, ProductNames.ProductNameServer));
		}
		if (!CheckIfSupportsApplicationVersion(updatedApplicationVersion))
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Application version [{0}] is unsupported and can not be updated", updatedApplicationVersion));
		}
		if (newAppFamily == null)
		{
			throw new InvalidOperationException("The application family field of the application database cannot be empty. To set the application family, use the ApplicationFamily parameter of the Set-NAVApplication cmdlet.");
		}
		if (newApplicationVersion != null && CheckIfSupportsApplicationVersion(newApplicationVersion))
		{
			return;
		}
		lock (lockObj)
		{
			Refresh();
			using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true))
			{
				using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
				StringBuilder updateTenantDatabaseApplicationVersionStatement = UpdateTenantDatabaseApplicationVersionStatement;
				updateTenantDatabaseApplicationVersionStatement.Append(" SET ");
				updateTenantDatabaseApplicationVersionStatement.Append("applicationfamily");
				updateTenantDatabaseApplicationVersionStatement.Append('=');
				updateTenantDatabaseApplicationVersionStatement.Append(ApplicationFamilyParamName);
				navSqlCommand.Command.Parameters.Add(new SqlParameter(ApplicationFamilyParamName, FieldSqlTypes[1], FieldLengths[1]));
				navSqlCommand.Command.Parameters[ApplicationFamilyParamName].Value = newAppFamily;
				if (newApplicationVersion != null)
				{
					updateTenantDatabaseApplicationVersionStatement.Append(" , ");
					updateTenantDatabaseApplicationVersionStatement.Append("applicationversion");
					updateTenantDatabaseApplicationVersionStatement.Append('=');
					updateTenantDatabaseApplicationVersionStatement.Append(ApplicationVersionParamName);
					navSqlCommand.Command.Parameters.Add(new SqlParameter(ApplicationVersionParamName, FieldSqlTypes[0], FieldLengths[0]));
					navSqlCommand.Command.Parameters[ApplicationVersionParamName].Value = newApplicationVersion;
				}
				updateTenantDatabaseApplicationVersionStatement.Append(" WHERE (");
				updateTenantDatabaseApplicationVersionStatement.Append("applicationversion");
				updateTenantDatabaseApplicationVersionStatement.Append('=');
				updateTenantDatabaseApplicationVersionStatement.Append(UpdateAppVersionParamName);
				navSqlCommand.Command.Parameters.Add(new SqlParameter(UpdateAppVersionParamName, FieldSqlTypes[0], FieldLengths[0]));
				navSqlCommand.Command.Parameters[UpdateAppVersionParamName].Value = updatedApplicationVersion;
				updateTenantDatabaseApplicationVersionStatement.Append(')');
				int num = navSqlCommand.ExecuteNonQuery(updateTenantDatabaseApplicationVersionStatement.ToString());
				if (num != 1)
				{
					throw new InvalidOperationException(updateTenantDatabaseApplicationVersionStatement.ToString());
				}
			}
			Refresh();
		}
	}

	public void DeleteSupportedApplicationVersion(string deletedAppVersion)
	{
		if (string.IsNullOrEmpty(deletedAppVersion))
		{
			return;
		}
		lock (lockObj)
		{
			Refresh();
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true);
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			StringBuilder deleteTenantDatabaseApplicationVersionStatement = DeleteTenantDatabaseApplicationVersionStatement;
			deleteTenantDatabaseApplicationVersionStatement.Append(" WHERE (");
			deleteTenantDatabaseApplicationVersionStatement.Append("applicationversion");
			deleteTenantDatabaseApplicationVersionStatement.Append('=');
			deleteTenantDatabaseApplicationVersionStatement.Append(ApplicationVersionParamName);
			navSqlCommand.Command.Parameters.Add(new SqlParameter(ApplicationVersionParamName, FieldSqlTypes[0], FieldLengths[0]));
			navSqlCommand.Command.Parameters[ApplicationVersionParamName].Value = deletedAppVersion;
			deleteTenantDatabaseApplicationVersionStatement.Append(')');
			int num = navSqlCommand.ExecuteNonQuery(deleteTenantDatabaseApplicationVersionStatement.ToString());
			if (num != 1)
			{
				throw new InvalidOperationException(deleteTenantDatabaseApplicationVersionStatement.ToString());
			}
		}
	}

	public int CountSupportedApplicationVersions()
	{
		lock (lockObj)
		{
			Refresh();
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true);
			StringBuilder countTenantDatabaseSupportedApplicationVersionStatement = CountTenantDatabaseSupportedApplicationVersionStatement;
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			object obj = navSqlCommand.ExecuteScalar(countTenantDatabaseSupportedApplicationVersionStatement.ToString());
			if (obj != null)
			{
				return (int)obj;
			}
			return 0;
		}
	}

	internal void TruncateApplicationVersions()
	{
		lock (lockObj)
		{
			Refresh();
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true);
			StringBuilder truncateApplicationVersionTableStatement = TruncateApplicationVersionTableStatement;
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000SX", Category.TenantDatabaseManagement, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Truncating : " + truncateApplicationVersionTableStatement);
			navSqlCommand.ExecuteNonQuery(truncateApplicationVersionTableStatement.ToString());
		}
	}

	internal static KeyValuePair<string, List<string>> GetSupportedFamilyAndVersions(NavDatabase database)
	{
		return GetSupportedFamiliesAndVersions(database).SingleOrDefault();
	}

	private static IDictionary<string, List<string>> GetSupportedFamiliesAndVersions(NavDatabase database)
	{
		IDictionary<string, List<string>> familyAndVersions = new Dictionary<string, List<string>>();
		if (!EstablishConnectionScope(database, out var connectionScope))
		{
			return familyAndVersions;
		}
		string commandGetVersionInfo = GenerateTenantDatabaseVersionCommand(connectionScope.Connection);
		commandGetVersionInfo = string.Format(CultureInfo.CurrentCulture, "IF OBJECT_ID('{0}','U') IS NOT NULL BEGIN {1} END", "[dbo].[$ndo$tenantdatabaseversion]", commandGetVersionInfo);
		try
		{
			return connectionScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey(commandGetVersionInfo, database), () => commandGetVersionInfo, (NavSqlCommand command) =>
			{
				using (command.ExecuteReader())
				{
					while (command.ReadNextRow())
					{
						string item = command.Reader.GetString(0);
						string key = command.Reader.GetString(1);
						List<string> value = null;
						if (!familyAndVersions.TryGetValue(key, out value))
						{
							value = new List<string>();
							familyAndVersions.Add(key, value);
						}
						value.Add(item);
						familyAndVersions[key] = value;
					}
					command.CloseReader(forceClose: false);
				}
				return familyAndVersions;
			});
		}
		catch (NavSqlException exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000QY", Category.TenantDatabaseManagement, exception);
			throw;
		}
		finally
		{
			connectionScope.Dispose();
		}
	}

	private void Read()
	{
		if (isReady)
		{
			return;
		}
		lock (lockObj)
		{
			if (isReady)
			{
				return;
			}
			using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(tenantDatabase);
			Read(conScope);
		}
	}

	private void Read(NavSqlConnectionScope conScope)
	{
		isReady = false;
		isTableExists = false;
		applicationVersion = NavGlobal.AppDatabase?.SqlDatabaseProperties?.ApplicationVersion;
		applicationFamily = NavGlobal.AppDatabase?.SqlDatabaseProperties?.ApplicationFamily;
		conScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("READTENANTDATABASEVERSION", tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, "\r\n                        IF OBJECT_ID(N'{0}', N'U') IS NOT NULL\r\n                            SELECT TOP 1 0, * FROM {0}\r\n                        ELSE\r\n                            SELECT 1", "[dbo].[$ndo$tenantdatabaseversion]"), (NavSqlCommand commandToExecute) =>
		{
			commandToExecute.ExecuteReader();
			if (!commandToExecute.ReadNextRow())
			{
				isTableExists = true;
				isReady = true;
				return true;
			}
			switch ((int)commandToExecute.Reader.GetValue(0))
			{
			case 0:
			{
				isTableExists = true;
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				for (int i = 0; i < commandToExecute.Reader.FieldCount; i++)
				{
					string name = commandToExecute.Reader.GetName(i);
					if (FieldNames.Contains(name, StringComparer.OrdinalIgnoreCase))
					{
						dictionary.Add(name, commandToExecute.Reader.IsDBNull(i) ? null : commandToExecute.Reader.GetValue(i));
					}
				}
				applicationVersion = NavSqlTenantDatabaseProperties.GetValueOrDefault(dictionary, 0, FieldNames, string.Empty);
				applicationFamily = NavSqlTenantDatabaseProperties.GetValueOrDefault(dictionary, 1, FieldNames, string.Empty);
				isReady = true;
				break;
			}
			case 1:
				isTableExists = false;
				isReady = true;
				break;
			default:
				throw new NotSupportedException();
			}
			commandToExecute.CloseReader(forceClose: false);
			return true;
		});
	}

	private void WriteFieldToDatabase(int fieldNo)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true);
		navSqlConnectionScope.Connection.ExecuteCommandAndCacheOnSuccess(new NavSqlStringBasedCommandCacheKey("UPDATETENANTDATABASEVERSION" + FieldNames[fieldNo].ToUpper(CultureInfo.InvariantCulture), tenantDatabase), () =>
		{
			StringBuilder stringBuilder = new StringBuilder("UPDATE ");
			stringBuilder.Append("[dbo].[$ndo$tenantdatabaseversion]");
			stringBuilder.Append(" SET ");
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, FieldNames[fieldNo]);
			stringBuilder.Append('=');
			stringBuilder.Append(NavSqlStatementHelper.ParameterName(fieldNo));
			return stringBuilder.ToString();
		}, (NavSqlCommand commandToExecute) =>
		{
			if (commandToExecute.Command.Parameters.Count == 0)
			{
				object valueAsObject = GetValueAsObject(fieldNo);
				string parameterName = NavSqlStatementHelper.ParameterName(fieldNo);
				SqlParameter value = NavSqlStatementHelper.CreateSqlParameter(parameterName, FieldTypes[fieldNo], SqlDataType.Default, FieldLengths[fieldNo], valueAsObject, isWildcardValue: false, isFilterValue: false);
				commandToExecute.Command.Parameters.Add(value);
			}
			else
			{
				commandToExecute.Command.Parameters[0].Value = GetValueAsObject(fieldNo);
			}
		}, (NavSqlCommand commandToExecute) =>
		{
			commandToExecute.ExecuteNonQuery();
			return true;
		});
	}

	private object GetValueAsObject(int fieldNo)
	{
		return fieldNo switch
		{
			0 => (object)(applicationVersion ?? NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion), 
			1 => applicationFamily ?? NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily, 
			_ => throw new ArgumentException("Illegal fieldNo specified"), 
		};
	}

	private void ThrowInvalidOperationExceptionIfDatabaseNotUpToDate()
	{
		if (!tenantDatabase.IsCurrentVersion)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Tenant {0} in database {1} (application version {2}) cannot be used. The tenant can be brought to the operational state by synchronizing the database.", tenantDatabase.Tenant.Id, tenantDatabase.ServerName, ApplicationVersion));
		}
	}

	private static string GenerateTenantDatabaseVersionCommand(NavSqlConnection connection)
	{
		StringBuilder getTenantDatabaseVersionsAndFamiliesStatement = GetTenantDatabaseVersionsAndFamiliesStatement;
		getTenantDatabaseVersionsAndFamiliesStatement.Append(NavSqlStatementHelper.GenerateSelectStatementHints(locking: false, noExpand: false, connection));
		getTenantDatabaseVersionsAndFamiliesStatement.Append(" ORDER BY ").Append("applicationversion").Append(" DESC");
		return getTenantDatabaseVersionsAndFamiliesStatement.ToString();
	}

	private static bool EstablishConnectionScope(NavDatabase database, out NavSqlConnectionScope connectionScope)
	{
		connectionScope = null;
		try
		{
			connectionScope = NavSqlConnectionScope.Create(database);
			return true;
		}
		catch (NavSqlException ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000Z3", Category.TenantDatabaseManagement, ex, Verbosity.Error, DiagnosticParameter.SystemMetadata("TryGetConnectionScope SQL error: " + ex.ErrorNumber));
			return false;
		}
		catch (NavCSideException exception)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000Z4", Category.TenantDatabaseManagement, exception);
		}
	}
}
