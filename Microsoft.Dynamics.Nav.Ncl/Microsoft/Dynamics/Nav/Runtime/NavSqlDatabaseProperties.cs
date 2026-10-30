using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlDatabaseProperties
{
	private readonly NavDatabase database;

	public const int CurrentDatabaseVersionNo = 130453;

	public const string ApplicationVersionFieldName = "applicationversion";

	public const string ApplicationFamilyFieldName = "applicationfamily";

	private const int BaselineDatabaseVersion = 100120;

	private const int FieldsAddedSinceBaseline = 2;

	private const string SelectSrvPropertiesStatement = "SELECT license FROM {0}";

	private const string SrvPropertyTableName = "[master].[dbo].[$ndo$srvproperty]";

	private const int SrvPropNoLicense = 0;

	public const string DbPropertyTableName = "$ndo$dbproperty";

	private const string ApplicationVersionFields = ", applicationversion, applicationfamily";

	private const string ApplicationVersionFieldIndices = ",@15,@16";

	private const string ApplicationVersionFieldsForCreate = "applicationversion       VARCHAR(50)     NOT NULL DEFAULT '',applicationfamily        VARCHAR(50)     NOT NULL DEFAULT '',";

	public const int ApplicationFamilyFieldLength = 50;

	private const string CreateDbPropertiesStatement = "CREATE TABLE [{0}].[{1}].[{2}] (programid                INTEGER         NOT NULL DEFAULT 0,chartable                IMAGE               NULL,maxrecsize               INTEGER         NOT NULL DEFAULT 0,maxloginattempts         INTEGER         NOT NULL DEFAULT 0,passwordexpindays        INTEGER         NOT NULL DEFAULT 0,supergroupid             VARCHAR(128)    NOT NULL DEFAULT '',unifiedloginallowed      TINYINT         NOT NULL DEFAULT 0,contype                  INTEGER         NOT NULL DEFAULT 0,shadowpwd                VARCHAR(128)    NOT NULL DEFAULT '',databasemagic            INTEGER         NOT NULL DEFAULT 0,databaseversionno        INTEGER         NOT NULL DEFAULT 0,clientdatabaseversionno  INTEGER         NOT NULL DEFAULT 0,maintainviews            INTEGER         NOT NULL DEFAULT 0,diagnostics              INTEGER         NOT NULL DEFAULT 0,identifiers              VARCHAR(2000)   NOT NULL DEFAULT '',maintainrelationships    INTEGER         NOT NULL DEFAULT 0,convertidentifiers       INTEGER         NOT NULL DEFAULT 0,invalididentifierchars   VARCHAR(128)    NOT NULL DEFAULT '',license                  IMAGE           NULL,checkcodepage            INTEGER         NOT NULL DEFAULT 0,quickfind                INTEGER         NOT NULL DEFAULT 0,maintaindefaults         INTEGER         NOT NULL DEFAULT 0,locktimeout              INTEGER         NOT NULL DEFAULT 0,locktimeoutperiod        INTEGER         NOT NULL DEFAULT 0,hardrowlock              INTEGER         NOT NULL DEFAULT 0,bufferedrows             INTEGER         NOT NULL DEFAULT 0,securityoption           INTEGER         NOT NULL DEFAULT 0,enabledforserver         INTEGER         NOT NULL DEFAULT 0,uidoffset                INTEGER         NOT NULL DEFAULT 0,{3}CONSTRAINT [PK$ndo$dbproperty] PRIMARY KEY CLUSTERED(programid))";

	private const string SelectDbPropertiesStatement = "SELECT TOP 1 chartable, maxrecsize, supergroupid, contype, databasemagic, databaseversionno, clientdatabaseversionno, identifiers, convertidentifiers, invalididentifierchars, license, checkcodepage, locktimeout, locktimeoutperiod, uidoffset{3}, sc.collation_name FROM [{0}].[{1}].[{2}] CROSS JOIN sys.columns AS sc WHERE sc.OBJECT_ID = OBJECT_ID('{2}') and sc.name = 'identifiers'";

	private const string SelectDatabaseVersionNoStatement = "SELECT TOP 1 databaseversionno FROM [{0}].[{1}].[{2}]";

	private const string InsertDbPropertiesStatement = "INSERT INTO [{0}].[{1}].[{2}] (chartable, maxrecsize, supergroupid, contype, databasemagic, databaseversionno, clientdatabaseversionno, identifiers, convertidentifiers, invalididentifierchars, license, checkcodepage, locktimeout, locktimeoutperiod, uidoffset{3}) VALUES (@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14{4})";

	private const string UpdateDbPropertiesStatement = "UPDATE [{0}].[{1}].[{2}] SET [{3}] = '{4}'";

	private const int DbPropNoCharTable = 0;

	private const int DbPropNoMaxRecSize = 1;

	private const int DbPropNoSuperGroupId = 2;

	private const int DbPropNoConType = 3;

	private const int DbPropNoDatabaseMagic = 4;

	private const int DbPropNoDatabaseVersionNo = 5;

	private const int DbPropNoClientDatabaseVersionNo = 6;

	private const int DbPropNoIdentifiers = 7;

	private const int DbPropNoConvertIdentifiers = 8;

	private const int DbPropNoInvalidIdentifierChars = 9;

	private const int DbPropNoLicense = 10;

	private const int DbPropNoCheckCodePage = 11;

	private const int DbPropNoLockTimeout = 12;

	private const int DbPropNoLockTimeoutPeriod = 13;

	private const int DbPropNoUIdOffset = 14;

	private const int DbPropNoApplicationVersion = 15;

	private const int DbPropNoApplicationFamily = 16;

	private const int DbPropNoCollationName = 17;

	private byte[] charTable;

	private int maxRecSize;

	private string superGroupId;

	private int conType;

	private int databaseMagic;

	private int databaseVersionNo;

	private int clientDatabaseVersionNo;

	private string identifiers;

	private int convertIdentifiers;

	private string invalididentifierChars;

	private byte[] databaseLicense;

	private int checkCodePage;

	private bool lockTimeoutEnabled;

	private int lockTimeout = -1;

	private int uidOffset;

	private string applicationVersion;

	private string applicationFamily;

	private byte[] serverLicense;

	private bool databasePropertiesReady;

	private bool serverPropertiesReady;

	private readonly object lockObj = new object();

	private string collation;

	public int DatabaseVersionNo
	{
		get
		{
			if (databaseVersionNo == 0)
			{
				databaseVersionNo = ReadDatabaseVersionNo();
			}
			return databaseVersionNo;
		}
	}

	public int DatabaseMagicNo
	{
		get
		{
			ReadDatabaseProperties();
			return databaseMagic;
		}
	}

	public string InvalidIdentifierChars
	{
		get
		{
			ReadDatabaseProperties();
			return invalididentifierChars;
		}
	}

	public bool LockTimeoutEnabled
	{
		get
		{
			ReadDatabaseProperties();
			return lockTimeoutEnabled;
		}
		set
		{
			lockTimeoutEnabled = value;
		}
	}

	public int LockTimeout
	{
		get
		{
			int value = ServerUserSettings.Instance.SqlLockTimeoutOverride.Value;
			if (value != 0)
			{
				return value;
			}
			ReadDatabaseProperties();
			return lockTimeout;
		}
		set
		{
			lockTimeout = value;
		}
	}

	public string Collation
	{
		get
		{
			ReadDatabaseProperties();
			return collation;
		}
	}

	public string ApplicationVersion
	{
		get
		{
			ReadDatabaseProperties();
			return applicationVersion;
		}
	}

	public string ApplicationFamily
	{
		get
		{
			ReadDatabaseProperties();
			return applicationFamily;
		}
	}

	internal bool DatabasePropertiesReady => databasePropertiesReady;

	private bool IsPreApplicationVersionDatabase => DatabaseVersionNo <= 100120;

	private int CalculateDbPropNoCollationNameIndex
	{
		get
		{
			if (!IsPreApplicationVersionDatabase)
			{
				return 17;
			}
			return 15;
		}
	}

	public NavSqlDatabaseProperties(NavDatabase database)
	{
		this.database = database;
	}

	private void ReadDatabaseProperties(bool forceRefresh = false)
	{
		lock (lockObj)
		{
			if (databasePropertiesReady && !forceRefresh)
			{
				return;
			}
			if (!NavSqlMetadataCommands.TableExists(database, "$ndo$dbproperty"))
			{
				throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.MissingSystemTable, "$ndo$dbproperty", database.DatabaseName, ProductNames.ProductNameShort));
			}
			using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database))
			{
				using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
				using (navSqlCommand.ExecuteReader(ComposeSelectDbPropertiesStatement()))
				{
					if (!navSqlCommand.ReadNextRow())
					{
						throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.MissingContentInSystemTable, "$ndo$dbproperty", database.DatabaseName));
					}
					if (!navSqlCommand.Reader.IsDBNull(0))
					{
						charTable = new byte[navSqlCommand.Reader.GetBytes(0, 0L, null, 0, 0)];
						navSqlCommand.Reader.GetBytes(0, 0L, charTable, 0, charTable.Length);
					}
					maxRecSize = navSqlCommand.Reader.GetInt32(1);
					superGroupId = navSqlCommand.Reader.GetString(2);
					conType = navSqlCommand.Reader.GetInt32(3);
					databaseMagic = navSqlCommand.Reader.GetInt32(4);
					databaseVersionNo = navSqlCommand.Reader.GetInt32(5);
					clientDatabaseVersionNo = navSqlCommand.Reader.GetInt32(6);
					identifiers = navSqlCommand.Reader.GetString(7);
					convertIdentifiers = navSqlCommand.Reader.GetInt32(8);
					invalididentifierChars = navSqlCommand.Reader.GetString(9);
					if (!navSqlCommand.Reader.IsDBNull(10))
					{
						databaseLicense = new byte[navSqlCommand.Reader.GetBytes(10, 0L, null, 0, 0)];
						navSqlCommand.Reader.GetBytes(10, 0L, databaseLicense, 0, databaseLicense.Length);
					}
					checkCodePage = navSqlCommand.Reader.GetInt32(11);
					lockTimeoutEnabled = navSqlCommand.Reader.GetInt32(12) != 0;
					lockTimeout = navSqlCommand.Reader.GetInt32(13);
					uidOffset = navSqlCommand.Reader.GetInt32(14);
					if (!IsPreApplicationVersionDatabase)
					{
						applicationVersion = navSqlCommand.Reader.GetString(15);
						applicationFamily = navSqlCommand.Reader.GetString(16);
					}
					collation = navSqlCommand.Reader.GetString(CalculateDbPropNoCollationNameIndex);
				}
			}
			databasePropertiesReady = true;
		}
	}

	private int ReadDatabaseVersionNo()
	{
		lock (lockObj)
		{
			if (!NavSqlMetadataCommands.TableExists(database, "$ndo$dbproperty"))
			{
				throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.MissingSystemTable, "$ndo$dbproperty", database.DatabaseName, ProductNames.ProductNameShort));
			}
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database);
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			int? num = (int?)navSqlCommand.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "SELECT TOP 1 databaseversionno FROM [{0}].[{1}].[{2}]", database.DatabaseName, "dbo", "$ndo$dbproperty"));
			if (!num.HasValue)
			{
				throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.MissingContentInSystemTable, "$ndo$dbproperty", database.DatabaseName));
			}
			return num.Value;
		}
	}

	private void ReadMasterServerPropertiesIfNotAlreadyRead()
	{
		lock (lockObj)
		{
			if (serverPropertiesReady || database.IsAzureDatabase)
			{
				return;
			}
			using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database))
			{
				using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
				using (navSqlCommand.ExecuteReader(string.Format(CultureInfo.InvariantCulture, "SELECT license FROM {0}", "[master].[dbo].[$ndo$srvproperty]")))
				{
					if (!navSqlCommand.ReadNextRow())
					{
						throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.MissingSystemTable, "[master].[dbo].[$ndo$srvproperty]", database.DatabaseName, ProductNames.ProductNameShort));
					}
					if (!navSqlCommand.Reader.IsDBNull(0))
					{
						serverLicense = new byte[navSqlCommand.Reader.GetBytes(0, 0L, null, 0, 0)];
						navSqlCommand.Reader.GetBytes(0, 0L, serverLicense, 0, serverLicense.Length);
					}
				}
			}
			serverPropertiesReady = true;
		}
	}

	private void CreateTable()
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, updateOrlockingActivity: true);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.ExecuteNonQuery(ComposeCreateDbPropertiesStatement());
	}

	private void InsertDatabaseProperties()
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, updateOrlockingActivity: true);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.SetCommandText(ComposeInsertDbPropertiesStatement());
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@0", charTable));
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@1", maxRecSize));
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@2", superGroupId));
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@3", conType));
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@4", databaseMagic));
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@5", databaseVersionNo));
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@6", clientDatabaseVersionNo));
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@7", identifiers));
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@8", convertIdentifiers));
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@9", invalididentifierChars));
		SqlParameter sqlParameter = new SqlParameter();
		sqlParameter.ParameterName = "@10";
		sqlParameter.SqlDbType = SqlDbType.Binary;
		if (databaseLicense == null)
		{
			sqlParameter.Value = DBNull.Value;
		}
		else
		{
			sqlParameter.Value = databaseLicense;
		}
		navSqlCommand.Command.Parameters.Add(sqlParameter);
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@11", checkCodePage));
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@12", lockTimeout != -1));
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@13", (lockTimeout != -1) ? lockTimeout : 0));
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@14", uidOffset));
		if (!IsPreApplicationVersionDatabase)
		{
			navSqlCommand.Command.Parameters.Add(new SqlParameter("@15", applicationVersion));
			navSqlCommand.Command.Parameters.Add(new SqlParameter("@16", applicationFamily));
		}
		navSqlCommand.ExecuteNonQuery();
	}

	private string ComposeInsertDbPropertiesStatement()
	{
		string text = (IsPreApplicationVersionDatabase ? string.Empty : ", applicationversion, applicationfamily");
		string text2 = (IsPreApplicationVersionDatabase ? string.Empty : ",@15,@16");
		return string.Format(CultureInfo.InvariantCulture, "INSERT INTO [{0}].[{1}].[{2}] (chartable, maxrecsize, supergroupid, contype, databasemagic, databaseversionno, clientdatabaseversionno, identifiers, convertidentifiers, invalididentifierchars, license, checkcodepage, locktimeout, locktimeoutperiod, uidoffset{3}) VALUES (@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14{4})", database.DatabaseName, "dbo", "$ndo$dbproperty", text, text2);
	}

	private string ComposeSelectDbPropertiesStatement()
	{
		string text = (IsPreApplicationVersionDatabase ? string.Empty : ", applicationversion, applicationfamily");
		return string.Format(CultureInfo.InvariantCulture, "SELECT TOP 1 chartable, maxrecsize, supergroupid, contype, databasemagic, databaseversionno, clientdatabaseversionno, identifiers, convertidentifiers, invalididentifierchars, license, checkcodepage, locktimeout, locktimeoutperiod, uidoffset{3}, sc.collation_name FROM [{0}].[{1}].[{2}] CROSS JOIN sys.columns AS sc WHERE sc.OBJECT_ID = OBJECT_ID('{2}') and sc.name = 'identifiers'", database.DatabaseName, "dbo", "$ndo$dbproperty", text);
	}

	private string ComposeCreateDbPropertiesStatement()
	{
		string text = (IsPreApplicationVersionDatabase ? string.Empty : "applicationversion       VARCHAR(50)     NOT NULL DEFAULT '',applicationfamily        VARCHAR(50)     NOT NULL DEFAULT '',");
		return string.Format(CultureInfo.InvariantCulture, "CREATE TABLE [{0}].[{1}].[{2}] (programid                INTEGER         NOT NULL DEFAULT 0,chartable                IMAGE               NULL,maxrecsize               INTEGER         NOT NULL DEFAULT 0,maxloginattempts         INTEGER         NOT NULL DEFAULT 0,passwordexpindays        INTEGER         NOT NULL DEFAULT 0,supergroupid             VARCHAR(128)    NOT NULL DEFAULT '',unifiedloginallowed      TINYINT         NOT NULL DEFAULT 0,contype                  INTEGER         NOT NULL DEFAULT 0,shadowpwd                VARCHAR(128)    NOT NULL DEFAULT '',databasemagic            INTEGER         NOT NULL DEFAULT 0,databaseversionno        INTEGER         NOT NULL DEFAULT 0,clientdatabaseversionno  INTEGER         NOT NULL DEFAULT 0,maintainviews            INTEGER         NOT NULL DEFAULT 0,diagnostics              INTEGER         NOT NULL DEFAULT 0,identifiers              VARCHAR(2000)   NOT NULL DEFAULT '',maintainrelationships    INTEGER         NOT NULL DEFAULT 0,convertidentifiers       INTEGER         NOT NULL DEFAULT 0,invalididentifierchars   VARCHAR(128)    NOT NULL DEFAULT '',license                  IMAGE           NULL,checkcodepage            INTEGER         NOT NULL DEFAULT 0,quickfind                INTEGER         NOT NULL DEFAULT 0,maintaindefaults         INTEGER         NOT NULL DEFAULT 0,locktimeout              INTEGER         NOT NULL DEFAULT 0,locktimeoutperiod        INTEGER         NOT NULL DEFAULT 0,hardrowlock              INTEGER         NOT NULL DEFAULT 0,bufferedrows             INTEGER         NOT NULL DEFAULT 0,securityoption           INTEGER         NOT NULL DEFAULT 0,enabledforserver         INTEGER         NOT NULL DEFAULT 0,uidoffset                INTEGER         NOT NULL DEFAULT 0,{3}CONSTRAINT [PK$ndo$dbproperty] PRIMARY KEY CLUSTERED(programid))", database.DatabaseName, "dbo", "$ndo$dbproperty", text);
	}

	private string ComposeUpdateDbPropertiesStatement(string fieldName, string fieldValue)
	{
		return string.Format(CultureInfo.InvariantCulture, "UPDATE [{0}].[{1}].[{2}] SET [{3}] = '{4}'", database.DatabaseName, "dbo", "$ndo$dbproperty", fieldName, fieldValue);
	}

	private void ValidateApplicationVersionChange(string value, bool skipVersionChangeValidation)
	{
		Version result = null;
		if (!Version.TryParse(value, out result))
		{
			throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.DbProperty_InvalidApplicationVersion, value));
		}
		if (!skipVersionChangeValidation)
		{
			Version version = Version.Parse(ApplicationVersion);
			if (result.Major != version.Major || result.Minor != version.Minor || result.Build != version.Build)
			{
				throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.DbProperty_NonRevisionApplicationVersionChange, value));
			}
			if (version.Revision > result.Revision)
			{
				throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.DbProperty_RevisionApplicationVersionChangeLower, value, ApplicationVersion));
			}
		}
	}

	private static string IncrementApplicationVersion(string applicationVersion)
	{
		Version version = Version.Parse(applicationVersion);
		if (version.Build == -1)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor + 1);
		}
		if (version.Revision == -1)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", version.Major, version.Minor, version.Build + 1);
		}
		return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision + 1);
	}

	public static void ReadPastProperties(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		if (num > 0)
		{
			reader.ReadBytes(num);
		}
		reader.ReadInt32();
		reader.ReadString();
		reader.ReadInt32();
		reader.ReadInt32();
		reader.ReadInt32();
		reader.ReadInt32();
		reader.ReadString();
		reader.ReadInt32();
		reader.ReadString();
		int num2 = reader.ReadInt32();
		if (num2 > 0)
		{
			reader.ReadBytes(num2);
		}
		reader.ReadInt32();
		reader.ReadInt32();
		reader.ReadInt32();
		reader.ReadString();
		reader.ReadString();
		reader.ReadString();
	}

	public void ReadPropertiesAndCreateTable(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		if (num > 0)
		{
			charTable = reader.ReadBytes(num);
		}
		maxRecSize = reader.ReadInt32();
		superGroupId = reader.ReadString();
		conType = reader.ReadInt32();
		databaseMagic = reader.ReadInt32();
		databaseVersionNo = reader.ReadInt32();
		clientDatabaseVersionNo = reader.ReadInt32();
		identifiers = reader.ReadString();
		convertIdentifiers = reader.ReadInt32();
		invalididentifierChars = reader.ReadString();
		int num2 = reader.ReadInt32();
		if (num2 > 0)
		{
			databaseLicense = reader.ReadBytes(num2);
		}
		checkCodePage = reader.ReadInt32();
		lockTimeout = reader.ReadInt32();
		uidOffset = reader.ReadInt32();
		if (!IsPreApplicationVersionDatabase)
		{
			applicationVersion = reader.ReadString();
			applicationFamily = reader.ReadString();
		}
		reader.ReadString();
		databasePropertiesReady = true;
		CheckDatabaseVersion();
		CreateTable();
		InsertDatabaseProperties();
		ReadDatabaseProperties(forceRefresh: true);
	}

	public void WriteProperties(BinaryWriter writer)
	{
		ReadDatabaseProperties();
		writer.Write((charTable != null) ? charTable.Length : 0);
		if (charTable != null)
		{
			writer.Write(charTable);
		}
		writer.Write(maxRecSize);
		writer.Write(superGroupId);
		writer.Write(conType);
		writer.Write(databaseMagic);
		writer.Write(databaseVersionNo);
		writer.Write(clientDatabaseVersionNo);
		writer.Write(identifiers);
		writer.Write(convertIdentifiers);
		writer.Write(invalididentifierChars);
		writer.Write((databaseLicense != null) ? databaseLicense.Length : 0);
		if (databaseLicense != null)
		{
			writer.Write(databaseLicense);
		}
		writer.Write(checkCodePage);
		writer.Write(lockTimeout);
		writer.Write(uidOffset);
		if (!IsPreApplicationVersionDatabase)
		{
			writer.Write(applicationVersion);
			writer.Write(applicationFamily);
		}
		writer.Write(collation);
	}

	public void CheckDatabaseVersion()
	{
		if (DatabaseVersionNo < 130453)
		{
			throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.DatabaseVersionTooOld, database.DatabaseName, database.DatabaseServer.ServerName, ProductNames.ProductNameClassicClient, ProductNames.ProductNameShort));
		}
		if (DatabaseVersionNo > 130453)
		{
			throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.DatabaseVersionTooNew, database.DatabaseName, database.DatabaseServer.ServerName, ProductNames.ProductNameShort));
		}
	}

	public byte[] CheckLicenseExists()
	{
		if (!NavEnvironment.Topology.UseServiceFabricServerLicense)
		{
			ReadDatabaseProperties();
			if (databaseLicense != null && databaseLicense.Length != 0)
			{
				return databaseLicense;
			}
			ReadMasterServerPropertiesIfNotAlreadyRead();
		}
		else if (serverLicense == null)
		{
			lock (lockObj)
			{
				if (serverLicense == null)
				{
					serverLicense = NavEnvironment.ServiceFabricServerLicense;
				}
			}
		}
		if (serverLicense != null && serverLicense.Length != 0)
		{
			return serverLicense;
		}
		throw new NavSqlException(Lang.NoLicenseFileExists);
	}

	public void MarkAsNotReady()
	{
		databasePropertiesReady = false;
		serverPropertiesReady = false;
	}

	public void UpdateDatabasePropertiesField(string fieldName, string value, bool force)
	{
		if ((fieldName == "applicationversion" && string.IsNullOrEmpty(value)) || (fieldName == "applicationfamily" && value == null))
		{
			return;
		}
		ReadDatabaseProperties();
		if (fieldName == "applicationversion")
		{
			ValidateApplicationVersionChange(value, force);
		}
		if (fieldName == "applicationfamily")
		{
			if (ServerUserSettings.Instance.Multitenant.Value && NavEnvironment.Instance.TenantDatabases.Configuration.TenantDatabaseCount() > 0)
			{
				throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.SetNavApplication_CannotSetApplcationFamilyWithMountedTenantDatabase));
			}
			if (ServerUserSettings.Instance.Multitenant.Value && NavEnvironment.Instance.Tenants.AllTenants().Any())
			{
				throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.SetNavApplication_CannotSetApplcationFamilyWithMountedTenants));
			}
			if (!force && !string.IsNullOrEmpty(applicationFamily))
			{
				throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.DbProperty_ExistingApplicationFamily, applicationFamily));
			}
		}
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, updateOrlockingActivity: true);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.ExecuteNonQuery(ComposeUpdateDbPropertiesStatement(fieldName, value));
	}

	public void IncrementApplicationVersion()
	{
		ReadDatabaseProperties();
		string value = IncrementApplicationVersion(applicationVersion);
		UpdateDatabasePropertiesField("applicationversion", value, force: true);
	}

	public void RefreshDatabaseProperties()
	{
		ReadDatabaseProperties(forceRefresh: true);
	}
}
