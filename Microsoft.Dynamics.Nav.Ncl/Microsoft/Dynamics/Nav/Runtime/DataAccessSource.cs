using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DataAccessSource
{
	private readonly NavSession session;

	private readonly bool allowAppDatabaseWrite;

	private readonly bool isCacheEnabled;

	private readonly GlobalFilters globalFilters = new GlobalFilters();

	private readonly DataAccessTableVersionTokens tableVersionTokens = DataAccessTableVersionTokens.CreateForNonTempTables();

	private Dictionary<int, DataAccess> virtualDataAccesses;

	private SessionTransactionManager sessionTransactionManager;

	private TransactionManager appTransactionManager;

	private DataAccess appDataAccess;

	private TransactionManager tenantTransactionManager;

	private DataAccess tenantDataAccess;

	public GlobalFilters GlobalFilters => globalFilters;

	public DataAccessTableVersionTokens TableVersionTokens => tableVersionTokens;

	public DataAccess TenantDataAccess
	{
		get
		{
			if (tenantDataAccess == null)
			{
				CreateTenantDataAccess();
			}
			return tenantDataAccess;
		}
	}

	public DataAccess AppDataAccess
	{
		get
		{
			if (appDataAccess == null)
			{
				CreateAppDataAccess();
			}
			return appDataAccess;
		}
	}

	public SessionTransactionManager SessionTransactionManager
	{
		get
		{
			if (sessionTransactionManager == null)
			{
				sessionTransactionManager = new SessionTransactionManager(AppTransactionManager, TenantTransactionManager);
			}
			return sessionTransactionManager;
		}
	}

	protected TransactionManager TenantTransactionManager
	{
		get
		{
			if (tenantTransactionManager == null)
			{
				CreateTenantDataAccess();
			}
			return tenantTransactionManager;
		}
	}

	protected TransactionManager AppTransactionManager
	{
		get
		{
			if (appTransactionManager == null)
			{
				CreateAppDataAccess();
			}
			return appTransactionManager;
		}
	}

	protected NavSession Session => session;

	protected bool AllowAppDatabaseWrite => allowAppDatabaseWrite;

	public DataAccessSource(NavSession session, bool allowAppDatabaseWrite = false, bool isCacheEnabled = true)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		this.session = session;
		this.allowAppDatabaseWrite = allowAppDatabaseWrite;
		this.isCacheEnabled = isCacheEnabled;
	}

	private void CreateTenantDataAccess()
	{
		DataProvider dataProvider = CreateTenantDataProvider();
		tenantTransactionManager = new TransactionManager(dataProvider);
		tenantDataAccess = new DataAccess(session, dataProvider, globalFilters, tableVersionTokens, new TransactionalDataCache(session, session.Database, tenantTransactionManager, tableVersionTokens, isCacheEnabled, isReadOnly: false));
	}

	private void CreateAppDataAccess()
	{
		DataProvider dataProvider = CreateAppDataProvider();
		appTransactionManager = new TransactionManager(dataProvider);
		appDataAccess = new DataAccess(session, dataProvider, globalFilters, tableVersionTokens, new TransactionalDataCache(session, session.AppDatabase, appTransactionManager, tableVersionTokens, isCacheEnabled, !AllowAppDatabaseWrite));
	}

	public virtual DataAccess GetDataAccessForTable(NCLMetaTable table, bool isTemp = false)
	{
		if (isTemp)
		{
			return CreateTempDataAccess(table);
		}
		if (table.IsVirtualTable)
		{
			return GetVirtualDataAccess(table);
		}
		switch (table.TableType)
		{
		case TableType.Normal:
			if (!table.IsAppTable)
			{
				return TenantDataAccess;
			}
			return AppDataAccess;
		case TableType.CRM:
			return session.TableConnectionManager.GetCurrentTableConnection<CrmTableConnection>(TableConnectionType.CRM).CreateDataAccess(table, globalFilters);
		case TableType.ExternalSQL:
			return session.TableConnectionManager.GetCurrentTableConnection<ExternalSqlTableConnection>(TableConnectionType.ExternalSQL).CreateDataAccess(globalFilters);
		case TableType.Exchange:
			return session.TableConnectionManager.GetCurrentTableConnection<ExchangeTableConnection>(TableConnectionType.Exchange).CreateDataAccess(table, globalFilters);
		case TableType.MicrosoftGraph:
			return session.TableConnectionManager.GetCurrentTableConnection<GraphTableConnection>(TableConnectionType.MicrosoftGraph).CreateDataAccess(table, globalFilters);
		case TableType.Query:
			return session.TableConnectionManager.GetCurrentTableConnection<QueryTableConnection>(TableConnectionType.Query).CreateDataAccess(table, globalFilters);
		default:
			throw new NotSupportedException(table.TableType.ToString());
		}
	}

	public DataAccess GetDataAccessForQuery(NCLMetaQuery nclMetaQuery)
	{
		return GetDataAccessForQuery(nclMetaQuery.QueryDefinition);
	}

	public DataAccess GetDataAccessForQuery(NCLMetaQueryDefinition queryDefinition)
	{
		DataAccess dataAccess = null;
		foreach (NCLMetaTable includedTable in queryDefinition.IncludedTables)
		{
			DataAccess dataAccessForTable = GetDataAccessForTable(includedTable);
			if (dataAccess == null)
			{
				dataAccess = dataAccessForTable;
			}
			else if (dataAccess != dataAccessForTable)
			{
				throw new NavQueryException(Lang.QueriesBetweenDataSourcesNotSupported);
			}
		}
		return dataAccess ?? TenantDataAccess;
	}

	protected virtual DataProvider CreateTenantDataProvider()
	{
		return new SqlTableDataProvider(session, session.Database);
	}

	protected virtual DataProvider CreateAppDataProvider()
	{
		return new SqlTableDataProvider(session, session.AppDatabase);
	}

	private DataAccess GetVirtualDataAccess(NCLMetaTable table)
	{
		DataAccess value = null;
		if (virtualDataAccesses == null)
		{
			virtualDataAccesses = new Dictionary<int, DataAccess>();
		}
		else
		{
			virtualDataAccesses.TryGetValue(table.TableId, out value);
		}
		if (value == null)
		{
			value = new DataAccess(session, GetVirtualDataProvider(table), globalFilters, tableVersionTokens, VirtualAndTempTransactionalDataCache.Instance);
			virtualDataAccesses.Add(table.TableId, value);
		}
		return value;
	}

	private DataAccess CreateTempDataAccess(NCLMetaTable table)
	{
		return new DataAccess(session, new TempTableDataProvider(session, table), globalFilters, DataAccessTableVersionTokens.CreateForTempTable(), VirtualAndTempTransactionalDataCache.Instance);
	}

	private DataProvider GetVirtualDataProvider(NCLMetaTable metaTable)
	{
		switch (metaTable.TableId)
		{
		case 2000000007:
			return DateDataProvider.Instance;
		case 2000000026:
			return IntegerDataProvider.Instance;
		case 2000000135:
			return TableSynchSetupDataProvider.GetInstanceForSession(session);
		case 2000000049:
			return CodeCoverageDataProvider.GetInstanceForSession(session);
		case 2000000101:
			return DebuggerCallStackDataProvider.Instance;
		case 2000000102:
			return DebuggerVariableDataProvider.Instance;
		case 2000000103:
			return DebuggerWatchValueDataProvider.Instance;
		case 2000000020:
			return DriveDataProvider.Instance;
		case 2000000055:
			return SidAccountIdDataProvider.Instance;
		case 2000000009:
			return SessionDataProvider.Instance;
		case 2000000110:
			if (ServerUserSettings.Instance.TrackSessionsInMemory.Value)
			{
				return ActiveSessionDataProvider.Instance;
			}
			return new SqlTableDataProvider(session, session.Database);
		case 2000000029:
			return new SystemObjectDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000038:
			return new AllObjDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000058:
			return new AllObjWithCaptionDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000041:
			return new FieldDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000136:
			return new TableDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000028:
			return new TableInformationDataProvider(session.Database);
		case 2000000063:
			return new KeyDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000137:
			return new CodeUnitDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000138:
			return new PageDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000139:
			return new ReportDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000140:
			return EventSubscriptionDataProvider.Instance;
		case 2000000141:
			return new TableRelationDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000142:
			return new QueryDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000022:
			return FileDataProvider.Instance;
		case 2000000048:
			return DatabaseDataProvider.Instance;
		case 2000000040:
			return LicenseInformationDataProvider.Instance;
		case 2000000043:
			return LicensePermissionDataProvider.Instance;
		case 2000000044:
			return PermissionRangeDataProvider.Instance;
		case 2000000039:
			return PrinterDataProvider.Instance;
		case 2000000045:
			return WindowsLanguageDataProvider.Instance;
		case 2000000164:
			return TimeZoneDataProvider.Instance;
		case 2000000167:
			return new AggregatePermissionSetDataProvider();
		case 2000000178:
			return new AllProfileDataProvider();
		case 2000000171:
			return new PageTableFieldDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000172:
			return new FieldTypeDesignerProvider();
		case 2000000154:
			return DatabaseLocksDataProvider.Instance;
		case 2000000193:
			return ApiWebServiceDataProvider.Instance;
		case 2000000097:
			return ApiWebhookEntityDataProvider.Instance;
		case 2000000192:
			return new PageControlFieldDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000143:
			return new PageActionDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000157:
			return NavAppExtraDataProvider.Instance;
		case 2000000202:
			return new AllControlFieldsDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000203:
			return new ReportDataItemsDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		case 2000000204:
			return new PageInfoAndFieldsDataProvider(NavGlobal.NCLMetadata, NavGlobal.MetadataProvider);
		default:
			throw new NotSupportedException();
		}
	}
}
