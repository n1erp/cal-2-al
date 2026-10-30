using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlTenantDatabaseProperties
{
	public const string TableName = "[dbo].[$ndo$tenantdatabaseproperty]";

	internal static readonly string[] FieldNames = new string[4] { "versionno", "collation", "percompanyschema", "applicationversion" };

	internal static readonly NavNclType[] FieldTypes = new NavNclType[4]
	{
		NavNclType.NavInteger,
		NavNclType.NavText,
		NavNclType.NavBoolean,
		NavNclType.NavText
	};

	internal static readonly int[] FieldLengths = new int[4] { 0, 128, 0, 50 };

	private const int VersionNoFieldNo = 0;

	private const int CollationFieldNo = 1;

	internal const int PerCompanySchemaFieldNo = 2;

	internal const int ApplicationVersionFieldNo = 3;

	private readonly NavDatabase tenantDatabase;

	private readonly object lockObj = new object();

	private bool isReady;

	private bool isTableExists;

	private int versionNo;

	private string collation;

	private bool perCompanySchema;

	private string applicationVersion;

	public int VersionNo
	{
		get
		{
			Read();
			return versionNo;
		}
	}

	public string Collation
	{
		get
		{
			Read();
			return collation;
		}
	}

	public bool PerCompanySchema
	{
		get
		{
			Read();
			return perCompanySchema;
		}
	}

	public string ApplicationVersion
	{
		get
		{
			Read();
			return applicationVersion;
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

	public bool IsReady => isReady;

	public NavSqlTenantDatabaseProperties(NavDatabase tenantDatabase)
	{
		if (tenantDatabase.DatabaseType != NavDatabaseType.Tenant)
		{
			throw new ArgumentException("The given database is not a tenant database.");
		}
		this.tenantDatabase = tenantDatabase;
	}

	public int NumberOfTenantsInThisDatabase()
	{
		using NavSqlConnectionScope scope = NavSqlConnectionScope.CreateForceNewConnection(tenantDatabase, updateOrlockingActivity: true);
		return NumberOfTenantsInThisDatabase(scope);
	}

	public static int NumberOfTenantsInThisDatabase(NavSqlConnectionScope scope)
	{
		int result = 0;
		using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(scope.Connection))
		{
			object obj = navSqlCommand.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "\r\n                        IF OBJECT_ID(N'{0}', N'U') IS NOT NULL\r\n                            SELECT COUNT(*) FROM {0}", "[dbo].[$ndo$tenantproperty]"));
			if (obj is int)
			{
				result = (int)obj;
			}
		}
		return result;
	}

	public void StoreDatabaseCollationSetting(string newCollation)
	{
		ThrowInvalidOperationExceptionIfDatabaseNotUpToDate();
		lock (lockObj)
		{
			Read();
			collation = newCollation;
			WriteFieldToDatabase(1);
		}
	}

	internal void StorePerCompanySchemaSetting(bool newPerCompanySchema)
	{
		ThrowInvalidOperationExceptionIfDatabaseNotUpToDate();
		lock (lockObj)
		{
			Read();
			perCompanySchema = newPerCompanySchema;
			WriteFieldToDatabase(2);
		}
	}

	public void StoreApplicationVersionSetting(string newApplicationVersion)
	{
		ThrowInvalidOperationExceptionIfDatabaseNotUpToDate();
		lock (lockObj)
		{
			Read();
			applicationVersion = newApplicationVersion;
			WriteFieldToDatabase(3);
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

	public void MarkAsNotReady()
	{
		isReady = false;
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
		versionNo = 0;
		perCompanySchema = true;
		try
		{
			collation = NavGlobal.AppDatabase.SqlSortingProperties.Collation;
			applicationVersion = NavGlobal.AppDatabase?.SqlDatabaseProperties?.ApplicationVersion;
		}
		catch (NavSqlException exception)
		{
			collation = string.Empty;
			applicationVersion = null;
			tenantDatabase.Tenant.Diagnostics.SendExceptionTag("00001LA", Category.Tenant, exception, Verbosity.Warning, DiagnosticParameter.SystemMetadata("Failed to retrieve application database information."));
		}
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.ExecuteReader(string.Format(CultureInfo.InvariantCulture, "\r\n                        IF OBJECT_ID(N'{0}', N'U') IS NOT NULL\r\n                            SELECT TOP 1 0, * FROM {0}\r\n                        ELSE\r\n                            SELECT 1", "[dbo].[$ndo$tenantdatabaseproperty]"));
		if (!navSqlCommand.ReadNextRow())
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Table {0} must not be empty.", "[dbo].[$ndo$tenantdatabaseproperty]"));
		}
		switch (Convert.ToInt32(navSqlCommand.Reader.GetValue(0), CultureInfo.InvariantCulture))
		{
		case 0:
		{
			isTableExists = true;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			for (int i = 0; i < navSqlCommand.Reader.FieldCount; i++)
			{
				string name = navSqlCommand.Reader.GetName(i);
				if (FieldNames.Contains(name, StringComparer.OrdinalIgnoreCase))
				{
					dictionary.Add(name, navSqlCommand.Reader.IsDBNull(i) ? null : navSqlCommand.Reader.GetValue(i));
				}
			}
			versionNo = GetValueOrDefault(dictionary, 0, FieldNames, 0);
			collation = GetValueOrDefault(dictionary, 1, FieldNames, string.Empty);
			perCompanySchema = GetValueOrDefault(dictionary, 2, FieldNames, defaultValue: true);
			applicationVersion = GetValueOrDefault(dictionary, 3, FieldNames, string.Empty);
			isReady = true;
			break;
		}
		case 1:
			isTableExists = false;
			versionNo = ReadLegacyTenantDatabaseVersion(conScope);
			isReady = true;
			break;
		default:
			throw new NotSupportedException();
		}
		navSqlCommand.CloseReader(forceClose: false);
	}

	internal static T GetValueOrDefault<T>(Dictionary<string, object> row, int fieldNo, string[] fieldNames, T defaultValue)
	{
		if (!row.TryGetValue(fieldNames[fieldNo], out var value))
		{
			return defaultValue;
		}
		if (value == null)
		{
			return defaultValue;
		}
		if (!(value is T) && (!(value is int) || !typeof(T).IsEnum))
		{
			if (!(value is byte) || !typeof(T).IsEquivalentTo(typeof(bool)))
			{
				throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture, "The object cannot be cast to {0}. Object type: {1}", typeof(T), value.GetType()));
			}
			value = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
		}
		return (T)value;
	}

	private void WriteFieldToDatabase(int fieldNo)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		StringBuilder stringBuilder = new StringBuilder("UPDATE ");
		stringBuilder.Append("[dbo].[$ndo$tenantdatabaseproperty]");
		stringBuilder.Append(" SET ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, FieldNames[fieldNo]);
		stringBuilder.Append('=');
		stringBuilder.Append(NavSqlStatementHelper.ParameterName(fieldNo));
		object valueAsObject = GetValueAsObject(fieldNo);
		string parameterName = NavSqlStatementHelper.ParameterName(fieldNo);
		SqlParameter value = NavSqlStatementHelper.CreateSqlParameter(parameterName, FieldTypes[fieldNo], SqlDataType.Default, FieldLengths[fieldNo], valueAsObject, isWildcardValue: false, isFilterValue: false);
		navSqlCommand.Command.Parameters.Add(value);
		navSqlCommand.ExecuteNonQuery(stringBuilder.ToString());
	}

	private object GetValueAsObject(int fieldNo)
	{
		return fieldNo switch
		{
			0 => (object)versionNo, 
			1 => collation, 
			2 => perCompanySchema, 
			3 => applicationVersion, 
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

	internal static int ReadLegacyTenantDatabaseVersion(NavSqlConnectionScope conScope, bool takeUpdateLock = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "IF OBJECT_ID(N'{0}', N'U') IS NOT NULL SELECT TOP 1 {1} FROM {0}", "[dbo].[$ndo$tenantproperty]", "tenantversionno"));
		if (takeUpdateLock)
		{
			stringBuilder.Append(" WITH (UPDLOCK)");
		}
		stringBuilder.Append(" ELSE SELECT 0");
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		return (int)navSqlCommand.ExecuteScalar(stringBuilder.ToString());
	}
}
