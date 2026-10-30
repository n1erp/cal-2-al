using System;
using System.Data.SqlClient;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ExternalSqlTableConnection : TableConnection
{
	private readonly string databaseServerName = ".";

	private readonly string databaseName;

	private readonly string userName;

	private readonly ProtectedDatabasePassword protectedPassword;

	private NavDatabaseServer databaseServer;

	private TransactionManager transactionManager;

	private DataProvider dataProvider;

	private NavTenant externalTenant;

	private readonly NavSession session;

	private readonly string name;

	private DataAccess dataAccess;

	internal override TableConnectionType ConnectionType => TableConnectionType.ExternalSQL;

	internal override bool SupportsTransactions => true;

	internal static ExternalSqlTableConnection Create(ITreeObject parent, NavSession session, string name, ExternalSqlTableConnectionSettings settings)
	{
		return new ExternalSqlTableConnection(parent, session, name, settings);
	}

	internal static ExternalSqlTableConnection Create(ITreeObject parent, NavSession session, string name, string connectionString)
	{
		SqlConnectionStringBuilder sqlConnectionStringBuilder;
		try
		{
			sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
		}
		catch (ArgumentException ex)
		{
			throw new NavNCLArgumentException(ex.Message);
		}
		if (session.CurrentSqlConnectionScope != null && session.CurrentSqlConnectionScope.IsTestScope)
		{
			throw new NavCSideException(Lang.CannotUseExternalSqlTablesWithTestIsolation);
		}
		string dataSource = sqlConnectionStringBuilder.DataSource;
		string initialCatalog = sqlConnectionStringBuilder.InitialCatalog;
		string userID = sqlConnectionStringBuilder.UserID;
		ProtectedDatabasePassword password = null;
		if (!string.IsNullOrEmpty(sqlConnectionStringBuilder.Password))
		{
			password = ProtectedDatabasePassword.Create(SecureStringExtension.CreateFromUnsecureString(sqlConnectionStringBuilder.Password));
		}
		return new ExternalSqlTableConnection(parent, session, name, new ExternalSqlTableConnectionSettings(name, dataSource, initialCatalog, userID, password, sqlConnectionStringBuilder.ConnectTimeout));
	}

	private ExternalSqlTableConnection(ITreeObject parent, NavSession session, string name, ExternalSqlTableConnectionSettings settings)
		: base(parent, name)
	{
		this.name = name;
		this.session = session;
		if (string.IsNullOrEmpty(settings.DatabaseServer) || string.IsNullOrEmpty(settings.DatabaseName))
		{
			throw new NavNCLArgumentException("Both database server and database name must be specified for external SQL table connections.");
		}
		databaseServerName = settings.DatabaseServer;
		databaseName = settings.DatabaseName;
		userName = settings.UserName;
		protectedPassword = settings.Password;
		databaseServer = NavEnvironment.Instance.DatabaseServers.GetOrAddDatabaseServerByName(databaseServerName, userName, protectedPassword);
		externalTenant = new NavExternalTenant(databaseServer, databaseName, settings);
	}

	internal DataAccess CreateDataAccess(GlobalFilters globalFilters)
	{
		CheckDisposed();
		if (dataAccess == null)
		{
			dataProvider = new SqlTableDataProvider(session, externalTenant.Database);
			transactionManager = new TransactionManager(dataProvider);
			session.DataAccessSource.SessionTransactionManager.AddTransactionManager(TableType.ExternalSQL, name, transactionManager);
			DataAccessTableVersionTokens tableVersionTokens = DataAccessTableVersionTokens.CreateForNonTempTables();
			dataAccess = new DataAccess(session, dataProvider, globalFilters, tableVersionTokens, new TransactionalDataCache(session, externalTenant.Database, transactionManager, tableVersionTokens, cacheEnabled: false, isReadOnly: false));
		}
		return dataAccess;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && !IsDisposed)
		{
			if (transactionManager != null)
			{
				TransactionManager instanceToRemove = transactionManager;
				transactionManager = null;
				session.DataAccessSource.SessionTransactionManager.RemoveTransactionManager(TableType.ExternalSQL, name, instanceToRemove);
			}
			if (externalTenant != null)
			{
				NavTenant navTenant = externalTenant;
				externalTenant = null;
				navTenant.Dispose();
			}
		}
		base.Dispose(disposing);
	}
}
