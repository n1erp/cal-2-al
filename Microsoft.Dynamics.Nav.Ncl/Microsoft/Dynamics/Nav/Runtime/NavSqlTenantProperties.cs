using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlTenantProperties
{
	public class AppInsightId
	{
		internal static readonly XmlSerializer Serializer = new XmlSerializer(typeof(AppInsightId[]), new XmlRootAttribute
		{
			ElementName = "items"
		});

		[XmlAttribute]
		public string Id { get; set; }

		[XmlAttribute]
		public Guid TenantId { get; set; }
	}

	public const string TableName = "[dbo].[$ndo$tenantproperty]";

	internal const string PrimaryKeyName = "PK$ndo$tenantproperty";

	internal static readonly string[] FieldNames = new string[9] { "tenantid", "license", "encryptionkeyfilename", "encryptionkeyhash", "tenantlocalid", "applicationversion", "tenanttype", "lastmodified", "appinsightsids" };

	internal static readonly NavNclType[] FieldTypes = new NavNclType[9]
	{
		NavNclType.NavText,
		NavNclType.NavBlob,
		NavNclType.NavText,
		NavNclType.NavText,
		NavNclType.NavInteger,
		NavNclType.NavText,
		NavNclType.NavInteger,
		NavNclType.NavDateTime,
		NavNclType.NavBlob
	};

	internal static readonly int[] FieldLengths = new int[9] { 128, 0, 128, 128, 0, 50, 0, 0, 0 };

	internal const int TenantIdFieldNo = 0;

	internal const int LicenseFieldNo = 1;

	internal const int EncryptionKeyFileNameFieldNo = 2;

	internal const int EncryptionKeyHashFieldNo = 3;

	internal const int TenantLocalIdNo = 4;

	internal const int TenantDataVersionFieldNo = 5;

	internal const int TenantTypeFieldNo = 6;

	internal const int LastModifiedFieldNo = 7;

	internal const int AppInsightsIdsFieldNo = 8;

	internal const string TenantIdColumnCollation = "Latin1_General_100_CI_AS";

	internal static readonly Version UninitializedVersion = new Version();

	private readonly NavDatabase tenantDatabase;

	private readonly string tenantId;

	private readonly object lockObj = new object();

	private bool isReady;

	private bool isNewTenant;

	private bool isTableExists;

	private string encryptionKeyFileName;

	private string encryptionKeyHash;

	private int? tenantLocalId;

	private string tenantDataVersion;

	private NavTenantType tenantType;

	private byte[] license;

	private DateTime lastModified;

	private Dictionary<string, Guid> appInsightsIdentifiers;

	public bool IsNewTenant
	{
		get
		{
			Read();
			return isNewTenant;
		}
	}

	public bool IsBufferTenant
	{
		get
		{
			Read();
			return TenantType == NavTenantType.Buffer;
		}
	}

	public NavTenantType TenantType
	{
		get
		{
			Read();
			return tenantType;
		}
	}

	public byte[] License
	{
		get
		{
			Read();
			return license;
		}
	}

	public string TenantId => tenantId;

	public int? TenantLocalId
	{
		get
		{
			Read();
			return tenantLocalId;
		}
	}

	public string TenantDataVersion
	{
		get
		{
			Read();
			return tenantDataVersion;
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

	public DateTime LastModified
	{
		get
		{
			Read();
			return lastModified;
		}
	}

	public ReadOnlyDictionary<string, Guid> AppInsightsIdentifiers
	{
		get
		{
			Read();
			return new ReadOnlyDictionary<string, Guid>(appInsightsIdentifiers);
		}
	}

	public NavSqlTenantProperties(NavDatabase tenantDatabase)
	{
		if (tenantDatabase.DatabaseType != NavDatabaseType.Tenant)
		{
			throw new ArgumentException("The given database is not a tenant database.");
		}
		this.tenantDatabase = tenantDatabase;
		if (string.IsNullOrEmpty(tenantDatabase.Tenant.Id))
		{
			throw new InvalidOperationException("Tenant Id cannot be null or empty");
		}
		tenantId = tenantDatabase.Tenant.Id;
	}

	public string OverwriteTenantId(NavSqlConnectionScope conScope, string newTenantId)
	{
		if (tenantDatabase.TenantDatabaseProperties.NumberOfTenantsInThisDatabase() > 1)
		{
			throw new NavTenantOverwriteTenantIdException(string.Format(CultureInfo.CurrentCulture, Lang.OverwriteTenantIdOnDatabaseWithMoreThanOneTenant, tenantDatabase.DatabaseName));
		}
		string oldTenantId = null;
		conScope.Connection.ExecuteCommandAndCacheOnSuccess(new NavSqlStringBasedCommandCacheKey("OVERWRITETENANTID-LOOKUP", tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, "IF OBJECT_ID(N'{0}', N'U') IS NOT NULL SELECT TOP 1 [{1}] FROM {0}", "[dbo].[$ndo$tenantproperty]", FieldNames[0]), (NavSqlCommand commandToExecute) =>
		{
		}, (NavSqlCommand commandToExecute) =>
		{
			oldTenantId = commandToExecute.ExecuteScalar<string>();
			return true;
		});
		conScope.Connection.ExecuteCommandAndCacheOnSuccess(new NavSqlStringBasedCommandCacheKey("OVERWRITETENANTID", tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, "\r\n                    IF OBJECT_ID(N'{0}', N'U') IS NOT NULL\r\n                        UPDATE {0} SET [{1}] = @0", "[dbo].[$ndo$tenantproperty]", FieldNames[0]), (NavSqlCommand commandToExecute) =>
		{
			if (commandToExecute.Command.Parameters.Count == 0)
			{
				SqlParameter value = NavSqlStatementHelper.CreateSqlParameter("@0", NavNclType.NavText, SqlDataType.Default, 128, newTenantId, isWildcardValue: false, isFilterValue: false);
				commandToExecute.Command.Parameters.Add(value);
			}
			else
			{
				commandToExecute.Command.Parameters[0].Value = newTenantId;
			}
		}, (NavSqlCommand commandToExecute) =>
		{
			commandToExecute.ExecuteScalar();
			return true;
		});
		return oldTenantId;
	}

	public void ChangeDismountedTenantId(TenantId newTenantId)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true);
		string currentTenantIdParameter = NavSqlStatementHelper.ParameterName(0);
		string newTenantIdParameter = NavSqlStatementHelper.ParameterName(1);
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("CHANGETENANTID", tenantDatabase), () => FormattableString.Invariant(FormattableStringFactory.Create("\r\n                        IF OBJECT_ID(N'{0}', N'U') IS NOT NULL\r\n                    UPDATE {1}\r\n                       SET [{2}] = {3},\r\n                           [{4}] = getutcdate()\r\n                     WHERE [{5}] = {6}", "[dbo].[$ndo$tenantproperty]", "[dbo].[$ndo$tenantproperty]", FieldNames[0], newTenantIdParameter, FieldNames[7], FieldNames[0], currentTenantIdParameter)), (NavSqlCommand commandToExecute) =>
		{
			SqlParameter value = NavSqlStatementHelper.CreateSqlParameter(currentTenantIdParameter, NavNclType.NavText, SqlDataType.Default, FieldLengths[0], currentTenantIdParameter, isWildcardValue: false, isFilterValue: false);
			commandToExecute.Command.Parameters.Add(value);
			value = NavSqlStatementHelper.CreateSqlParameter(newTenantIdParameter, NavNclType.NavText, SqlDataType.Default, FieldLengths[0], newTenantId.ToString(), isWildcardValue: false, isFilterValue: false);
			commandToExecute.Command.Parameters.Add(value);
		}, (NavSqlCommand commandToExecute) =>
		{
			commandToExecute.Command.Parameters[0].Value = tenantId;
			commandToExecute.Command.Parameters[1].Value = newTenantId.ToString();
		}, (NavSqlCommand commandToExecute) => commandToExecute.ExecuteNonQuery());
		new DatabaseUninstalledAppRepository(tenantDatabase, navSqlConnectionScope).ChangeTenantId(tenantId, newTenantId.ToString());
	}

	public bool RegisterNewTenant()
	{
		lock (lockObj)
		{
			Refresh();
			if (!isNewTenant)
			{
				return false;
			}
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("INSERT INTO ").Append("[dbo].[$ndo$tenantproperty]");
			stringBuilder.Append('(');
			for (int i = 0; i < FieldNames.Length; i++)
			{
				if (IsFieldRequiredForNewTenant(i))
				{
					NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, FieldNames[i]);
					stringBuilder.Append(',');
				}
			}
			stringBuilder.Length--;
			stringBuilder.Append(") VALUES (");
			int num = 0;
			using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection))
			{
				for (int j = 0; j < FieldNames.Length; j++)
				{
					if (IsFieldRequiredForNewTenant(j))
					{
						object valueAsObject = GetValueAsObject(j);
						string text = NavSqlStatementHelper.ParameterName(num++);
						SqlParameter value = NavSqlStatementHelper.CreateSqlParameter(text, FieldTypes[j], SqlDataType.Default, FieldLengths[j], valueAsObject, isWildcardValue: false, isFilterValue: false);
						navSqlCommand.Command.Parameters.Add(value);
						stringBuilder.Append(text);
						stringBuilder.Append(',');
					}
				}
				stringBuilder.Length--;
				stringBuilder.Append(')');
				navSqlCommand.ExecuteNonQuery(stringBuilder.ToString());
			}
			Refresh();
			tenantDatabase.TenantCompanies.InsertSharedCompany();
			return true;
		}
	}

	public bool UnregisterDeletedTenant()
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("DELETE FROM ").Append("[dbo].[$ndo$tenantproperty]");
		stringBuilder.Append(" WHERE ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, FieldNames[0]);
		stringBuilder.Append(" = @1");
		stringBuilder.Append(" AND ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, FieldNames[6]);
		stringBuilder.Append(" = @2; ");
		stringBuilder.Append(" SELECT @@ROWCOUNT");
		bool flag;
		using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection))
		{
			SqlParameter value = NavSqlStatementHelper.CreateSqlParameter("@1", FieldTypes[0], SqlDataType.Default, FieldLengths[0], tenantDatabase.Tenant.Id, isWildcardValue: false, isFilterValue: false);
			navSqlCommand.Command.Parameters.Add(value);
			SqlParameter value2 = NavSqlStatementHelper.CreateSqlParameter("@2", FieldTypes[6], SqlDataType.Default, FieldLengths[6], NavTenantType.Deleted, isWildcardValue: false, isFilterValue: false);
			navSqlCommand.Command.Parameters.Add(value2);
			navSqlCommand.SetCommandText(stringBuilder.ToString());
			flag = navSqlCommand.ExecuteScalar<int>() == 1;
		}
		if (flag)
		{
			tenantDatabase.TenantCompanies.DeleteSharedCompany();
		}
		return flag;
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

	private bool IsFieldRequiredForNewTenant(int fieldNo)
	{
		switch (fieldNo)
		{
		case 1:
		case 4:
		case 7:
			return false;
		case 6:
			return tenantDatabase.IsSharedTenantSchema;
		default:
			return true;
		}
	}

	private object GetValueAsObject(int fieldNo)
	{
		return fieldNo switch
		{
			0 => tenantId, 
			1 => license, 
			2 => ((object)encryptionKeyFileName) ?? ((object)DBNull.Value), 
			3 => ((object)encryptionKeyHash) ?? ((object)DBNull.Value), 
			4 => tenantLocalId, 
			5 => ((object)tenantDataVersion) ?? ((object)DBNull.Value), 
			6 => tenantType, 
			7 => lastModified, 
			8 => SerializeAppInsightsGuids(appInsightsIdentifiers), 
			_ => throw new ArgumentException("Illegal fieldNo specified"), 
		};
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
		isNewTenant = false;
		isTableExists = false;
		license = null;
		encryptionKeyFileName = null;
		encryptionKeyHash = null;
		tenantLocalId = null;
		tenantDataVersion = null;
		appInsightsIdentifiers = new Dictionary<string, Guid>();
		conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("READTENANTPROPERTIES", tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, "\r\n                IF OBJECT_ID(N'{0}', N'U') IS NOT NULL\r\n                    IF (SELECT COUNT(*) FROM {0} WHERE tenantid = @0 COLLATE {1}) != 0\r\n                        SELECT TOP 1 0, * FROM {0} WHERE tenantid = @0 COLLATE {1}\r\n                    ELSE\r\n                        SELECT 1\r\n                ELSE\r\n                    SELECT 2", "[dbo].[$ndo$tenantproperty]", "Latin1_General_100_CI_AS"), (NavSqlCommand commandToExecute) =>
		{
			commandToExecute.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 63));
		}, (NavSqlCommand commandToExecute) =>
		{
			commandToExecute.Command.Parameters[0].Value = tenantId;
		}, (NavSqlCommand commandToExecute) => ReadTenantProperties(commandToExecute));
	}

	private bool ReadTenantProperties(NavSqlCommand commandToExecute)
	{
		commandToExecute.ExecuteReader();
		commandToExecute.ReadNextRow();
		switch (Convert.ToInt32(commandToExecute.Reader.GetValue(0), CultureInfo.InvariantCulture))
		{
		case 0:
		{
			isNewTenant = false;
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
			license = NavSqlTenantDatabaseProperties.GetValueOrDefault<byte[]>(dictionary, 1, FieldNames, null);
			encryptionKeyFileName = NavSqlTenantDatabaseProperties.GetValueOrDefault<string>(dictionary, 2, FieldNames, null);
			encryptionKeyHash = NavSqlTenantDatabaseProperties.GetValueOrDefault<string>(dictionary, 3, FieldNames, null);
			tenantLocalId = NavSqlTenantDatabaseProperties.GetValueOrDefault<int?>(dictionary, 4, FieldNames, null);
			tenantDataVersion = NavSqlTenantDatabaseProperties.GetValueOrDefault<string>(dictionary, 5, FieldNames, null);
			tenantType = NavSqlTenantDatabaseProperties.GetValueOrDefault(dictionary, 6, FieldNames, NavTenantType.Normal);
			lastModified = DateTime.SpecifyKind(NavSqlTenantDatabaseProperties.GetValueOrDefault(dictionary, 7, FieldNames, SqlDateTime.MinValue.Value), DateTimeKind.Utc);
			appInsightsIdentifiers = DeserializeAppInsightsGuids(NavSqlTenantDatabaseProperties.GetValueOrDefault<byte[]>(dictionary, 8, FieldNames, null));
			break;
		}
		case 1:
			isNewTenant = true;
			isTableExists = true;
			tenantDataVersion = NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion;
			break;
		case 2:
			isNewTenant = true;
			isTableExists = false;
			break;
		default:
			throw new NotSupportedException();
		}
		isReady = true;
		commandToExecute.CloseReader(forceClose: false);
		return true;
	}

	public void SetEncryptionKeyFileName(string newEncryptionKeyFileName)
	{
		lock (lockObj)
		{
			Read();
			encryptionKeyFileName = newEncryptionKeyFileName;
			WriteFieldToDatabase(2);
		}
	}

	public void SetEncryptionKeyHash(string newEncryptionKeyHash)
	{
		lock (lockObj)
		{
			Read();
			encryptionKeyHash = newEncryptionKeyHash;
			WriteFieldToDatabase(3);
		}
	}

	public void SetTenantType(NavTenantType newTenantType)
	{
		if (!tenantDatabase.IsSharedTenantSchema)
		{
			throw new NavNotSupportedException("SetTenantType is only supported in shared schema database");
		}
		lock (lockObj)
		{
			Read();
			tenantType = newTenantType;
			WriteFieldToDatabase(6);
		}
	}

	public void SetTenantDataVersion(string newApplicationVersion)
	{
		lock (lockObj)
		{
			Read();
			tenantDataVersion = newApplicationVersion;
			WriteFieldToDatabase(5);
		}
	}

	public void AddAppInsightsIdentifier(string licenseUniqueIdentifier, Guid uniqueIdentifier)
	{
		lock (lockObj)
		{
			Read();
			appInsightsIdentifiers.Add(licenseUniqueIdentifier, uniqueIdentifier);
			WriteFieldToDatabase(8);
		}
	}

	public void ClearAppInsightsIdentifiers()
	{
		lock (lockObj)
		{
			Read();
			appInsightsIdentifiers = new Dictionary<string, Guid>();
			WriteFieldToDatabase(8);
		}
	}

	public string GetEncryptionKeyFileName()
	{
		Refresh();
		return encryptionKeyFileName;
	}

	public string GetEncryptionKeyHash()
	{
		Refresh();
		return encryptionKeyHash;
	}

	public string MarkAsSoftRemoved()
	{
		string newTenantId = tenantId + Guid.NewGuid().ToString("N");
		newTenantId = newTenantId.Substring(0, Math.Min(63, newTenantId.Length));
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true))
		{
			navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("MarkAsSoftRemoved", tenantDatabase), () =>
			{
				StringBuilder stringBuilder = new StringBuilder("UPDATE ");
				stringBuilder.Append("[dbo].[$ndo$tenantproperty]");
				stringBuilder.Append(" SET ");
				NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, FieldNames[0]);
				stringBuilder.Append('=');
				stringBuilder.Append(NavSqlStatementHelper.ParameterName(1));
				stringBuilder.Append(" ,");
				NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, FieldNames[6]);
				stringBuilder.Append('=');
				stringBuilder.Append(NavSqlStatementHelper.ParameterName(2));
				stringBuilder.Append(" ,");
				NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, FieldNames[7]);
				stringBuilder.Append("= getutcdate()");
				stringBuilder.Append(" WHERE ");
				NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, FieldNames[0]);
				stringBuilder.Append("=");
				stringBuilder.Append(NavSqlStatementHelper.ParameterName(0));
				return stringBuilder.ToString();
			}, (NavSqlCommand commandToExecute) =>
			{
				commandToExecute.Command.Parameters.Add(NavSqlStatementHelper.CreateSqlParameter(NavSqlStatementHelper.ParameterName(0), FieldTypes[0], SqlDataType.Default, FieldLengths[0], null, isWildcardValue: false, isFilterValue: false));
				commandToExecute.Command.Parameters.Add(NavSqlStatementHelper.CreateSqlParameter(NavSqlStatementHelper.ParameterName(1), FieldTypes[0], SqlDataType.Default, FieldLengths[0], null, isWildcardValue: false, isFilterValue: false));
				commandToExecute.Command.Parameters.Add(NavSqlStatementHelper.CreateSqlParameter(NavSqlStatementHelper.ParameterName(2), FieldTypes[6], SqlDataType.Default, FieldLengths[6], null, isWildcardValue: false, isFilterValue: false));
			}, (NavSqlCommand commandToExecute) =>
			{
				commandToExecute.Command.Parameters[0].Value = tenantId;
				commandToExecute.Command.Parameters[1].Value = newTenantId;
				commandToExecute.Command.Parameters[2].Value = NavTenantType.SoftDeleted;
			}, (NavSqlCommand commandToExecute) =>
			{
				commandToExecute.ExecuteNonQuery();
				return true;
			});
		}
		return newTenantId;
	}

	private void WriteFieldToDatabase(int fieldNo)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true);
		navSqlConnectionScope.Connection.ExecuteCommandAndCacheOnSuccess(new NavSqlStringBasedCommandCacheKey("UPDATETENANTPROPERTIES" + FieldNames[fieldNo].ToUpper(CultureInfo.InvariantCulture), tenantDatabase), () =>
		{
			StringBuilder stringBuilder = new StringBuilder("UPDATE ");
			stringBuilder.Append("[dbo].[$ndo$tenantproperty]");
			stringBuilder.Append(" SET ");
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, FieldNames[fieldNo]);
			stringBuilder.Append('=');
			stringBuilder.Append(NavSqlStatementHelper.ParameterName(fieldNo));
			stringBuilder.Append(", ");
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, FieldNames[7]);
			stringBuilder.Append("= getutcdate()");
			stringBuilder.Append(" WHERE ");
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, FieldNames[0]);
			stringBuilder.Append("=");
			stringBuilder.Append(NavSqlStatementHelper.ParameterName(0));
			return stringBuilder.ToString();
		}, (NavSqlCommand commandToExecute) =>
		{
			if (commandToExecute.Command.Parameters.Count == 0)
			{
				object valueAsObject = GetValueAsObject(fieldNo);
				string parameterName = NavSqlStatementHelper.ParameterName(fieldNo);
				SqlParameter value = NavSqlStatementHelper.CreateSqlParameter(parameterName, FieldTypes[fieldNo], SqlDataType.Default, FieldLengths[fieldNo], valueAsObject, isWildcardValue: false, isFilterValue: false);
				commandToExecute.Command.Parameters.Add(value);
				string parameterName2 = NavSqlStatementHelper.ParameterName(0);
				SqlParameter value2 = NavSqlStatementHelper.CreateSqlParameter(parameterName2, FieldTypes[0], SqlDataType.Default, FieldLengths[0], tenantId, isWildcardValue: false, isFilterValue: false);
				commandToExecute.Command.Parameters.Add(value2);
			}
			else
			{
				commandToExecute.Command.Parameters[0].Value = GetValueAsObject(fieldNo);
				commandToExecute.Command.Parameters[1].Value = tenantId;
			}
		}, (NavSqlCommand commandToExecute) =>
		{
			commandToExecute.ExecuteNonQuery();
			return true;
		});
	}

	internal static byte[] SerializeAppInsightsGuids(Dictionary<string, Guid> applicationInsightsIds)
	{
		if (applicationInsightsIds == null || applicationInsightsIds.Count == 0)
		{
			return new byte[0];
		}
		MemoryStream memoryStream = new MemoryStream();
		using (StreamWriter textWriter = new StreamWriter(memoryStream, Encoding.UTF8))
		{
			AppInsightId.Serializer.Serialize(textWriter, applicationInsightsIds.Select((KeyValuePair<string, Guid> kv) => new AppInsightId
			{
				Id = kv.Key,
				TenantId = kv.Value
			}).ToArray());
		}
		return memoryStream.ToArray();
	}

	internal static Dictionary<string, Guid> DeserializeAppInsightsGuids(byte[] applicationInsightsIds)
	{
		if (applicationInsightsIds == null || applicationInsightsIds.Length == 0)
		{
			return new Dictionary<string, Guid>();
		}
		using StreamReader textReader = new StreamReader(new MemoryStream(applicationInsightsIds), Encoding.UTF8);
		using XmlReader xmlReader = XmlHelper.SafeXmlReader(textReader);
		return ((AppInsightId[])AppInsightId.Serializer.Deserialize(xmlReader)).ToDictionary((AppInsightId i) => i.Id, (AppInsightId i) => i.TenantId);
	}
}
