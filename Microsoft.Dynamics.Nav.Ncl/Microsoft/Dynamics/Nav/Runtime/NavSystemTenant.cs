using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;
using Microsoft.Dynamics.Nav.EventSubscription;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavSystemTenant : NavTenant
{
	private readonly NCLMetadata nclMetadata;

	private readonly INCLCodeLoader nclCodeLoader;

	private readonly INavAppClrTypeRetriever navAppClrTypeRetriever;

	private readonly INCLObjectXmlMetadataLoader nclObjectXmlMetadataLoader;

	private readonly NavDataUpgradeMetadata upgradeMetadata;

	private readonly NavEventSubscriptionMetadata eventSubscriptionMetadata;

	private readonly MetadataProvider metadataProvider;

	private readonly INavAddInProvider addInProvider;

	private readonly EntitlementProvider entitlementProvider;

	private readonly SyncMetadataResolver syncMetadataResolver;

	private readonly NavAppAddInAssemblyManager navAppAddInAssemblyManager;

	private readonly NavSqlTableAliasMap navSqlTableAliasMap;

	private readonly NavAppSymbolReferenceCache symbolReferenceCache;

	private readonly Lazy<IDotNetResolverFactory> lazyDotNetResolverFactory;

	private readonly INavAppTenantOperationManagementProvider navAppTenantOperationManagementProvider;

	public override NavTenantType TenantType => NavTenantType.System;

	public NCLMetadata NCLMetadata => ExposeField(() => nclMetadata);

	public INCLCodeLoader NCLCodeLoader => ExposeField(() => nclCodeLoader);

	public INavAppClrTypeRetriever NavAppClrTypeRetriever => ExposeField(() => navAppClrTypeRetriever);

	internal INCLObjectXmlMetadataLoader NCLObjectXmlMetadataLoader => ExposeField(() => nclObjectXmlMetadataLoader);

	public NavDataUpgradeMetadata UpgradeMetadata => ExposeField(() => upgradeMetadata);

	public NavEventSubscriptionMetadata EventSubscriptionMetadata => ExposeField(() => eventSubscriptionMetadata);

	public MetadataProvider MetadataProvider => ExposeField(() => metadataProvider);

	public INavAddInProvider AddInProvider => ExposeField(() => addInProvider);

	public NavAppAddInAssemblyManager NavAppAddInAssemblyManager => ExposeField(() => navAppAddInAssemblyManager);

	public IDotNetResolverFactory DotNetResolverFactory => lazyDotNetResolverFactory.Value;

	public NavSqlTableAliasMap NavSqlTableAliasMap => ExposeField(() => navSqlTableAliasMap);

	public SyncMetadataResolver SyncMetadataResolver => ExposeField(() => syncMetadataResolver);

	internal EntitlementProvider EntitlementProvider => ExposeField(() => entitlementProvider);

	internal NavAppSymbolReferenceCache SymbolReferenceCache => ExposeField(() => symbolReferenceCache);

	internal INavAppTenantOperationManagementProvider NavAppTenantOperationManagementProvider => ExposeField(() => navAppTenantOperationManagementProvider);

	public NavSystemTenant(NavDatabaseServer databaseServer, string appDatabaseName)
		: base(new NavTenantSettings("nav-systemapplication", appDatabaseName, databaseServer.ServerName, databaseServer.UserName, databaseServer.Password, null, allowAppDatabaseWrite: true), databaseServer)
	{
		navSqlTableAliasMap = new NavSqlTableAliasMap();
		INCLObjectMetadataCache cache = null;
		if (NavEnvironment.Instance.AllowCachingOfMetadataXml)
		{
			NCLObjectMetadataLRUCache<XmlDocument> lruDocumentCache = new NCLObjectMetadataLRUCache<XmlDocument>(new LRUCache(ServerUserSettings.Instance.XmlMetadataCacheSize.Value), new NCLObjectMetadataCacheKeyComparer());
			cache = new NCLObjectMetadataCache(new NCLObjectMetadataXmlCache(lruDocumentCache), new NCLObjectMetadataBlobCache());
		}
		nclCodeLoader = new NCLCodeLoader(NavEnvironment.Instance.TemporaryPathHelper);
		navAppClrTypeRetriever = new NavAppClrTypeRetriever(NavEnvironment.Instance.TemporaryPathHelper.AppAssemblyPath, NavEnvironment.Instance.NavAppMetadataRetriever, ServerUserSettings.Instance.EnableDebugging.Value);
		nclObjectXmlMetadataLoader = new NCLObjectXmlMetadataLoader((int sysTab) => nclMetadata.GetMetaTableById(sysTab), Database, NavEnvironment.Instance.NavAppMetadataRetriever, cache);
		nclMetadata = new NCLMetadata(Database, nclObjectXmlMetadataLoader, nclCodeLoader, navAppClrTypeRetriever);
		upgradeMetadata = new NavDataUpgradeMetadata(Database);
		List<NavEventExclusionFilter> list = new List<NavEventExclusionFilter>();
		list.AddRange(NavUpgradeEventConfiguration.ExclusionFilters);
		list.AddRange(NavInstallEventConfiguration.ExclusionFilters);
		List<NavEventSubscriptionSettingsEntry> list2 = new List<NavEventSubscriptionSettingsEntry>();
		list2.AddRange(NavUpgradeEventConfiguration.SettingsEntries);
		List<NavEventErrorReporter> list3 = new List<NavEventErrorReporter>();
		list3.AddRange(NavUpgradeEventConfiguration.ErrorReporters);
		List<INavEventSubscriptionObserver> staticSubscriptionObservers = new List<INavEventSubscriptionObserver>
		{
			new NavInstallUpgradeSubscriptionObserver()
		};
		eventSubscriptionMetadata = new NavEventSubscriptionMetadata(() => Database.Tenant, list, list2, list3, staticSubscriptionObservers);
		metadataProvider = new MetadataProvider();
		addInProvider = new NavAddInProvider();
		symbolReferenceCache = new NavAppSymbolReferenceCache();
		lazyDotNetResolverFactory = new Lazy<IDotNetResolverFactory>(() =>
		{
			NavAppCompilationAssemblyLocator assemblyLocator = new NavAppCompilationAssemblyLocator(NavCurrentThread.Session.SystemTenant.AddInProvider);
			return new DotNetResolverFactory(assemblyLocator);
		});
		navAppTenantOperationManagementProvider = new NavAppTenantOperationManagementProvider();
		syncMetadataResolver = new SyncMetadataResolver(NavEnvironment.Instance.NavAppMetadataRetriever, nclObjectXmlMetadataLoader);
		entitlementProvider = new EntitlementProvider(Database);
		navAppAddInAssemblyManager = new NavAppAddInAssemblyManager(new DatabaseNavAppAddInRetriever());
		NavAutomationHelper.RegisterNavAppsServerAddInAssemblyResolver(navAppAddInAssemblyManager.GetNavAppServerAddInAssemblyUsingCurrentContext);
		NavAutomationHelper.RegisterServerAddInResolver(addInProvider.ResolveAddInFromAddInTable);
	}

	protected override NavDatabase CreateDatabase()
	{
		return new NavDatabase(this, DatabaseServer, TenantSettings.DatabaseName, NavDatabaseType.Application);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			eventSubscriptionMetadata.Dispose();
		}
	}

	private T ExposeField<T>(Func<T> fieldRetriever)
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException("NavSystemTenant");
		}
		return fieldRetriever();
	}
}
