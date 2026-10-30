using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class PermissionManager
{
	private static readonly ApplicationObjectId WildcardTableData = new ApplicationObjectId(ObjectType.TableData, 0);

	private readonly PermissionCache permissionCache = new PermissionCache();

	private readonly NavSession session;

	private readonly Func<NavSession, ApplicationObjectId, PermissionMask> licensePermissionRetriever = (NavSession navSession, ApplicationObjectId objectId) => navSession.Database.SecurityAndLicense.GetLicensePermissions(objectId);

	private readonly Func<NavSession, bool> isPermissionSystemEnabledFunc = (NavSession navSession) => navSession.Database.SecurityAndLicense.IsPermissionSystemEnabled();

	private readonly Func<NavSession, bool> isSuperForAllCompaniesFunc;

	private readonly Func<NavSession, bool> isNavAdminUserFunc;

	private readonly Dictionary<ApplicationObjectId, Dictionary<ApplicationObjectId, PermissionSet>> simplePermissionSets = new Dictionary<ApplicationObjectId, Dictionary<ApplicationObjectId, PermissionSet>>();

	private readonly Dictionary<ApplicationObjectId, PermissionSet> simplePermissionSetNoCallingObject = new Dictionary<ApplicationObjectId, PermissionSet>();

	private readonly Dictionary<ApplicationObjectId, Dictionary<ApplicationObjectId, LicensePermissionSet>> licensePermissionSets = new Dictionary<ApplicationObjectId, Dictionary<ApplicationObjectId, LicensePermissionSet>>();

	private readonly Dictionary<ApplicationObjectId, LicensePermissionSet> licensePermissionSetNoCallingObject = new Dictionary<ApplicationObjectId, LicensePermissionSet>();

	private int localSetupVersion;

	private readonly Dictionary<int, FilterFieldDictionary> directReadSecurityFilters = new Dictionary<int, FilterFieldDictionary>();

	private readonly Dictionary<int, FilterFieldDictionary> directInsertSecurityFilters = new Dictionary<int, FilterFieldDictionary>();

	private readonly Dictionary<int, FilterFieldDictionary> directModifySecurityFilters = new Dictionary<int, FilterFieldDictionary>();

	private readonly Dictionary<int, FilterFieldDictionary> directDeleteSecurityFilters = new Dictionary<int, FilterFieldDictionary>();

	private readonly Dictionary<int, FilterFieldDictionary> directAndInderectReadSecurityFilters = new Dictionary<int, FilterFieldDictionary>();

	private readonly Dictionary<int, FilterFieldDictionary> directAndInderectInsertSecurityFilters = new Dictionary<int, FilterFieldDictionary>();

	private readonly Dictionary<int, FilterFieldDictionary> directAndInderectModifySecurityFilters = new Dictionary<int, FilterFieldDictionary>();

	private readonly Dictionary<int, FilterFieldDictionary> directAndInderectDeleteSecurityFilters = new Dictionary<int, FilterFieldDictionary>();

	private int id;

	private static int lastId;

	private IList<Tuple<string, AppId>> tuplesOfRoleIdAndAppId;

	private readonly object obj = new object();

	private static int NextId => Interlocked.Increment(ref lastId);

	internal int Id => id;

	public int CompanyNameToken { get; private set; }

	private bool HasAllPermissions
	{
		get
		{
			if (!isSuperForAllCompaniesFunc(session))
			{
				return !isPermissionSystemEnabledFunc(session);
			}
			return true;
		}
	}

	internal bool HasAdminUserPermissions => isNavAdminUserFunc(session);

	private IList<Tuple<string, AppId>> TuplesOfRoleIdAndAppId
	{
		get
		{
			if (tuplesOfRoleIdAndAppId == null)
			{
				lock (obj)
				{
					if (tuplesOfRoleIdAndAppId == null)
					{
						tuplesOfRoleIdAndAppId = GetTenantPermissions(session.Tenant.Database);
					}
				}
			}
			return tuplesOfRoleIdAndAppId;
		}
	}

	public PermissionManager(NavSession session, int companyNameToken)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		this.session = session;
		CompanyNameToken = companyNameToken;
		localSetupVersion = this.session.Database.PermissionSetupMonitor.SetupVersion;
		isSuperForAllCompaniesFunc = (NavSession navSession) => navSession.Permissions.IsSuperForAllCompanies;
		isNavAdminUserFunc = (NavSession navSession) => navSession.User.IsNavAdminUser;
		id = NextId;
		if (FeatureSwitches.Instance.EnableSecurityTracing)
		{
			session?.Diagnostics.SendTraceTag("000010E", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "PermissionManager constructed with Id:{0}, by sessionId:{1}.", id.ToString(CultureInfo.InvariantCulture), session.Id.ToString(CultureInfo.InvariantCulture)));
		}
	}

	internal PermissionManager(NavSession session, int companyNameToken, Func<NavSession, ApplicationObjectId, PermissionMask> licensePermissionRetriever, Func<NavSession, bool> isPermissionSystemEnabledFunc, Func<NavSession, bool> isSuperForAllCompaniesFunc, Func<NavSession, bool> isNavAdminUserFunc)
		: this(session, companyNameToken)
	{
		this.licensePermissionRetriever = licensePermissionRetriever;
		this.isPermissionSystemEnabledFunc = isPermissionSystemEnabledFunc;
		this.isSuperForAllCompaniesFunc = isSuperForAllCompaniesFunc;
		this.isNavAdminUserFunc = isNavAdminUserFunc;
	}

	public string GetObjectName(ApplicationObjectId objectId)
	{
		return session.GetObjectNameForErrorMessage(objectId);
	}

	public IPermissionSet GetPermissionSet(NavApplicationObjectBase callingObject, IEnumerable<ApplicationObjectId> applicationObjects)
	{
		if (applicationObjects == null)
		{
			throw new ArgumentNullException("applicationObjects");
		}
		EnsurePermissionSetupVersionCurrent();
		return CreateCompositePermissionSet(callingObject, applicationObjects);
	}

	public IPermissionSet GetPermissionSet(NavApplicationObjectBase callingObject, ApplicationObjectId applicationObject)
	{
		EnsurePermissionSetupVersionCurrent();
		if (!HasAllPermissions)
		{
			return CreateSimplePermissionSet(callingObject, applicationObject);
		}
		return CreateLicensePermissionSet(callingObject, applicationObject);
	}

	public IPermissionSet GetLicensePermissionSet(NavApplicationObjectBase callingObject, ApplicationObjectId applicationObject)
	{
		EnsurePermissionSetupVersionCurrent();
		return CreateLicensePermissionSet(callingObject, applicationObject);
	}

	private IPermissionSet CreateLicensePermissionSet(NavApplicationObjectBase callingObject, ApplicationObjectId applicationObject)
	{
		Dictionary<ApplicationObjectId, LicensePermissionSet> value = null;
		LicensePermissionSet value2;
		if (callingObject != null)
		{
			if (!licensePermissionSets.TryGetValue(applicationObject, out value))
			{
				value = new Dictionary<ApplicationObjectId, LicensePermissionSet>();
				licensePermissionSets.Add(applicationObject, value);
			}
			if (value.TryGetValue(callingObject.ObjectId, out value2))
			{
				return value2;
			}
		}
		else if (licensePermissionSetNoCallingObject.TryGetValue(applicationObject, out value2))
		{
			return value2;
		}
		value2 = new LicensePermissionSet(applicationObject, callingObject, this);
		if (callingObject != null)
		{
			value.Add(callingObject.ObjectId, value2);
		}
		else
		{
			licensePermissionSetNoCallingObject.Add(applicationObject, value2);
		}
		return value2;
	}

	private IPermissionSet CreateSimplePermissionSet(NavApplicationObjectBase callingObject, ApplicationObjectId applicationObject)
	{
		Dictionary<ApplicationObjectId, PermissionSet> value = null;
		PermissionSet value2;
		if (callingObject != null)
		{
			if (!simplePermissionSets.TryGetValue(applicationObject, out value))
			{
				value = new Dictionary<ApplicationObjectId, PermissionSet>();
				simplePermissionSets.Add(applicationObject, value);
			}
			if (value.TryGetValue(callingObject.ObjectId, out value2))
			{
				return value2;
			}
		}
		else if (simplePermissionSetNoCallingObject.TryGetValue(applicationObject, out value2))
		{
			return value2;
		}
		value2 = new PermissionSet(applicationObject, callingObject, this);
		if (callingObject != null)
		{
			value.Add(callingObject.ObjectId, value2);
		}
		else
		{
			simplePermissionSetNoCallingObject.Add(applicationObject, value2);
		}
		return value2;
	}

	private IPermissionSet CreateCompositePermissionSet(NavApplicationObjectBase callingObject, IEnumerable<ApplicationObjectId> applicationObjects)
	{
		CompositePermissionSet compositePermissionSet = new CompositePermissionSet();
		foreach (ApplicationObjectId applicationObject in applicationObjects)
		{
			compositePermissionSet.AddPermissionSet(GetPermissionSet(callingObject, applicationObject));
		}
		if (compositePermissionSet.PermissionSets.Count != 1)
		{
			return compositePermissionSet;
		}
		return compositePermissionSet.PermissionSets.First();
	}

	public bool HasMaximizedPermissions(int tableId)
	{
		return session.Permissions.HasMaximizedPermissions(tableId);
	}

	public PermissionMask GetLicensePermissions(ApplicationObjectId objectId)
	{
		return licensePermissionRetriever(session, objectId);
	}

	public PermissionMask GetEntitlementPermission(NavUser user, ApplicationObjectId objectId)
	{
		return EntitlementPermissionSet.GetEntitlementPermissions(user, objectId);
	}

	public PermissionMask GetPermissions(ApplicationObjectId objectId)
	{
		return GetPermissions(NavCurrentThread.Session.User, objectId);
	}

	internal PermissionMask GetPermissions(NavUser user, ApplicationObjectId objectId)
	{
		EnsurePermissionSetupVersionCurrent();
		if (permissionCache.TryGetValue(objectId, out var result))
		{
			return result;
		}
		PermissionMask licensePermissions = GetLicensePermissions(objectId);
		if (licensePermissions == PermissionMask.None)
		{
			if (FeatureSwitches.Instance.EnableSecurityTracing)
			{
				session?.Diagnostics.SendTraceTag("0000100", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "GetLicensePermissions returned None.\n Object Type {0}\n Object Id {1}\n PermissionMamager Id {2}\n SessionId {3}", objectId.ObjectType, objectId.ObjectNumber, id, session.Id));
			}
			permissionCache.AddOrSet(objectId, PermissionMask.None);
			return PermissionMask.None;
		}
		licensePermissions = ApplyEntitlementPermission(licensePermissions, user, objectId);
		if (licensePermissions == PermissionMask.None)
		{
			if (FeatureSwitches.Instance.EnableSecurityTracing)
			{
				session?.Diagnostics.SendTraceTag("0000101", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "ApplyEntitlementPermission returned None.\n Object Type {0}\n Object Id {1}\n PermissionMamager Id {2}\n SessionId {3}", objectId.ObjectType, objectId.ObjectNumber, id, session.Id));
			}
			permissionCache.AddOrSet(objectId, PermissionMask.None);
			return PermissionMask.None;
		}
		PermissionMask rolePermissions = GetRolePermissions(user, objectId);
		if (rolePermissions == PermissionMask.None && FeatureSwitches.Instance.EnableSecurityTracing)
		{
			session?.Diagnostics.SendTraceTag("0000102", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "GetRolePermissions returned None.\n Object Type {0}\n Object Id {1}\n PermissionMamager Id {2}\n SessionId {3}", objectId.ObjectType, objectId.ObjectNumber, id, session.Id));
		}
		rolePermissions |= PermissionMask.IncludedLimitedUsage | PermissionMask.ExcludedLimitedUsage;
		PermissionMask permissionMask = rolePermissions & licensePermissions;
		permissionCache.AddOrSet(objectId, permissionMask);
		return permissionMask;
	}

	public PermissionMask ApplyEntitlementPermission(PermissionMask permission, NavUser user, ApplicationObjectId objectId)
	{
		if (!ServerUserSettings.Instance.EnableMembershipEntitlement.Value)
		{
			return permission;
		}
		return permission.Combine(GetEntitlementPermission(user, objectId));
	}

	private PermissionMask GetRolePermissions(NavUser user, ApplicationObjectId objectId)
	{
		HashSet<NavRole> roles = null;
		if (NavTestExecution.UseEffectivePermissions())
		{
			roles = new HashSet<NavRole>();
			session.TestExecution.EffectivePermissionSets.ForEach((string rid) =>
			{
				HashSet<NavRole> hashSet = NavUserPermissions.FetchRolesFromRoleId(session, rid);
				if (hashSet.Any())
				{
					roles.UnionWith(hashSet);
				}
			});
		}
		else
		{
			NavUserPermissions navUserPermissions = ((user.Id == NavCurrentThread.Session.User.Id) ? session.Permissions : new NavUserPermissions(user, NavCurrentThread.Session));
			roles = navUserPermissions.GetRoles(CompanyNameToken);
		}
		if (NavTestExecution.IsInTestMode())
		{
			foreach (Tuple<string, AppId> item in TuplesOfRoleIdAndAppId)
			{
				roles.UnionWith(NavUserPermissions.FetchRolesFromRoleIdAndAppId(session, item.Item1, item.Item2));
			}
		}
		PermissionMask permissionMask = PermissionMask.None;
		foreach (NavRole item2 in roles)
		{
			if (item2.Permissions.TryGetValue(objectId, out var result))
			{
				permissionMask |= result;
			}
			if (item2.Permissions.TryGetValue(new ApplicationObjectId(objectId.ObjectType, 0), out result))
			{
				permissionMask |= result;
			}
			if (permissionMask == PermissionMask.MaxDirect)
			{
				break;
			}
		}
		return permissionMask;
	}

	private IList<Tuple<string, AppId>> GetTenantPermissions(NavDatabase tenantDatabase)
	{
		List<Tuple<string, AppId>> list = new List<Tuple<string, AppId>>();
		using NavSqlConnectionScope tenantConScope = NavSqlConnectionScope.CreateForceNewConnection(tenantDatabase, TransactionType.Browse);
		list.AddRange(GetTenantPermissions(tenantConScope));
		return list;
	}

	private IList<Tuple<string, AppId>> GetTenantPermissions(NavSqlConnectionScope tenantConScope)
	{
		List<Tuple<string, AppId>> list = new List<Tuple<string, AppId>>();
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(tenantConScope.Connection);
		string commandText = string.Format(CultureInfo.InvariantCulture, "IF (OBJECT_ID('[{0}].[Tenant Permission Set]', 'U') IS NOT NULL)\r\n    SELECT \r\n        [Role ID], [App ID]\r\n    FROM \r\n        [{0}].[Tenant Permission Set]", "dbo");
		navSqlCommand.SetCommandText(commandText);
		navSqlCommand.PrepareStatement();
		using (navSqlCommand.ExecuteReader())
		{
			while (navSqlCommand.ReadNextRow())
			{
				Tuple<string, AppId> item = new Tuple<string, AppId>(navSqlCommand.Reader.GetString(0), AppId.op_Implicit(navSqlCommand.Reader.GetGuid(1)));
				list.Add(item);
			}
			return list;
		}
	}

	internal FilterFieldDictionary GetSecurityFilters(int tableId, SecurityFilterType securityFilterType, NavApplicationObjectBase callingObject, NavApplicationObjectBase securableObject)
	{
		EnsurePermissionSetupVersionCurrent();
		PermissionMask permissionMask = (PermissionMask)(callingObject?.GetIndirectPermissionMask(tableId) ?? 0);
		if (securableObject != null)
		{
			permissionMask = (PermissionMask)((int)permissionMask | (int)(NavApplicationObjectBase.FindDeclaringApplicationObject(securableObject)?.GetIndirectPermissionMask(tableId) ?? 0));
		}
		switch (securityFilterType)
		{
		case SecurityFilterType.Read:
			if (permissionMask.HasFlag(PermissionMask.IndirectRead))
			{
				return GetSecurityFilters(tableId, PermissionMask.Read | PermissionMask.IndirectRead, directAndInderectReadSecurityFilters);
			}
			return GetSecurityFilters(tableId, PermissionMask.Read, directReadSecurityFilters);
		case SecurityFilterType.Insert:
			if (permissionMask.HasFlag(PermissionMask.IndirectInsert))
			{
				return GetSecurityFilters(tableId, PermissionMask.Insert | PermissionMask.IndirectInsert, directAndInderectInsertSecurityFilters);
			}
			return GetSecurityFilters(tableId, PermissionMask.Insert, directInsertSecurityFilters);
		case SecurityFilterType.Modify:
			if (permissionMask.HasFlag(PermissionMask.IndirectModify))
			{
				return GetSecurityFilters(tableId, PermissionMask.Modify | PermissionMask.IndirectModify, directAndInderectModifySecurityFilters);
			}
			return GetSecurityFilters(tableId, PermissionMask.Modify, directModifySecurityFilters);
		case SecurityFilterType.Delete:
			if (permissionMask.HasFlag(PermissionMask.IndirectDelete))
			{
				return GetSecurityFilters(tableId, PermissionMask.Delete | PermissionMask.IndirectDelete, directAndInderectDeleteSecurityFilters);
			}
			return GetSecurityFilters(tableId, PermissionMask.Delete, directDeleteSecurityFilters);
		default:
			return FilterFieldDictionary.Empty;
		}
	}

	internal void Clear()
	{
		localSetupVersion = session.Database.PermissionSetupMonitor.SetupVersion;
		permissionCache.Clear();
		ClearCompositeSecurityFilters();
		simplePermissionSetNoCallingObject.Clear();
		simplePermissionSets.Clear();
		licensePermissionSets.Clear();
		licensePermissionSetNoCallingObject.Clear();
	}

	private FilterFieldDictionary GetSecurityFilters(int tableId, PermissionMask permissionMask, Dictionary<int, FilterFieldDictionary> securityFiltersCollection)
	{
		if (securityFiltersCollection.TryGetValue(tableId, out var value))
		{
			return value;
		}
		foreach (NavRole role in session.Permissions.GetRoles(CompanyNameToken))
		{
			if (role.Permissions.TryGetValue(WildcardTableData, out var result) && (result & permissionMask) != 0)
			{
				securityFiltersCollection[tableId] = FilterFieldDictionary.Empty;
				break;
			}
			if (role.Permissions.TryGetValue(new ApplicationObjectId(ObjectType.TableData, tableId), out result) && (result & permissionMask) != 0)
			{
				FilterFieldDictionary securityFiltersForTable = role.Permissions.GetSecurityFiltersForTable(tableId);
				if (CombineSecurityFiltersByField(tableId, securityFiltersCollection, securityFiltersForTable))
				{
					break;
				}
			}
		}
		if (!securityFiltersCollection.TryGetValue(tableId, out value) || value == null)
		{
			value = (securityFiltersCollection[tableId] = FilterFieldDictionary.Empty);
		}
		return value;
	}

	private static bool CombineSecurityFiltersByField(int tableId, Dictionary<int, FilterFieldDictionary> securityFiltersCollection, FilterFieldDictionary tableSecurityFilters)
	{
		if (tableSecurityFilters == null)
		{
			return false;
		}
		if (securityFiltersCollection.ContainsKey(tableId))
		{
			securityFiltersCollection[tableId] = CombineSecurityFiltersByField(securityFiltersCollection[tableId], tableSecurityFilters);
		}
		else
		{
			securityFiltersCollection[tableId] = tableSecurityFilters;
		}
		if (securityFiltersCollection[tableId] == FilterFieldDictionary.Empty)
		{
			return true;
		}
		return false;
	}

	private static FilterFieldDictionary CombineSecurityFiltersByField(FilterFieldDictionary fieldFilters1, FilterFieldDictionary fieldFilters2)
	{
		if (fieldFilters1 == FilterFieldDictionary.Empty || fieldFilters2 == FilterFieldDictionary.Empty)
		{
			return FilterFieldDictionary.Empty;
		}
		List<Tuple<INavFieldMetadata, FilterExpression>> list = new List<Tuple<INavFieldMetadata, FilterExpression>>();
		for (int i = 0; i < fieldFilters1.Items.Count(); i++)
		{
			INavFieldMetadata field = fieldFilters1.Items[i].Item1;
			if (!fieldFilters2.Items.Any((Tuple<INavFieldMetadata, FilterExpression> t) => t.Item1 == field))
			{
				break;
			}
			FilterExpression item = FilterExpression.Or(fieldFilters1.Items[i].Item2, fieldFilters2.Items.Single((Tuple<INavFieldMetadata, FilterExpression> t) => t.Item1 == field).Item2);
			list.Add(new Tuple<INavFieldMetadata, FilterExpression>(field, item));
		}
		if (list.Count == 0)
		{
			return FilterFieldDictionary.Empty;
		}
		return new FilterFieldDictionary(list);
	}

	private void ClearCompositeSecurityFilters()
	{
		directReadSecurityFilters.Clear();
		directInsertSecurityFilters.Clear();
		directModifySecurityFilters.Clear();
		directDeleteSecurityFilters.Clear();
		directAndInderectReadSecurityFilters.Clear();
		directAndInderectInsertSecurityFilters.Clear();
		directAndInderectModifySecurityFilters.Clear();
		directAndInderectDeleteSecurityFilters.Clear();
	}

	private void EnsurePermissionSetupVersionCurrent()
	{
		if (localSetupVersion < session.Database.PermissionSetupMonitor.SetupVersion)
		{
			Clear();
		}
	}
}
