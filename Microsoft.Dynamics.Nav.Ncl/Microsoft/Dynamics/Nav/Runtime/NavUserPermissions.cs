using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavUserPermissions
{
	public const string SuperRoleId = "SUPER";

	public const string SecurityRoleId = "SECURITY";

	public const string BasicRoleId = "BASIC";

	private readonly Dictionary<int, HashSet<NavRole>> roles;

	private readonly List<PermissionManager> permissionManagers;

	private readonly Func<int, HashSet<NavRole>> rolesFunc;

	private readonly Dictionary<int, int> maximizedPermissions = new Dictionary<int, int>();

	private readonly NavUser user;

	private readonly NavSession session;

	private int localSetupVersion;

	private bool? isSuperUserForAllCompanies;

	public bool IsSuperForAllCompanies
	{
		get
		{
			if (NavTestExecution.UseEffectivePermissions())
			{
				return false;
			}
			if (session.User.IsNavAdminUser)
			{
				return true;
			}
			EnsurePermissionSetupVersionCorrect();
			if (!session.Database.SecurityAndLicense.IsPermissionSystemEnabled())
			{
				return true;
			}
			if (!isSuperUserForAllCompanies.HasValue)
			{
				GetRoles(0);
			}
			return isSuperUserForAllCompanies == true;
		}
	}

	public bool IsSecurityForAllCompanies => HasRole(0, "SECURITY", PermissionScope.System, AppId.None);

	public NavUserPermissions(NavUser user, NavSession session)
	{
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		this.user = user;
		this.session = session;
		roles = new Dictionary<int, HashSet<NavRole>>();
		permissionManagers = new List<PermissionManager>();
		localSetupVersion = this.session.Database.PermissionSetupMonitor.SetupVersion;
		rolesFunc = (int companyNameToken) =>
		{
			if (!roles.TryGetValue(companyNameToken, out var value))
			{
				value = FetchRoles(companyNameToken);
				roles.Add(companyNameToken, value);
			}
			return value;
		};
	}

	public void ClearPermissions()
	{
		if (FeatureSwitches.Instance.EnableSecurityTracing)
		{
			session.Diagnostics.SendTraceTag("000004H", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Clearing permissions in all permission managers");
		}
		foreach (PermissionManager permissionManager in permissionManagers)
		{
			permissionManager?.Clear();
		}
	}

	public HashSet<NavRole> GetRoles(int companyNameToken)
	{
		EnsurePermissionSetupVersionCorrect();
		return rolesFunc(companyNameToken);
	}

	public bool HasRole(int companyNameToken, string roleId, PermissionScope scope, AppId appId)
	{
		IEnumerable<NavRole> source = from nr in GetRoles(companyNameToken)
			where nr.Id == roleId && nr.Scope == scope && (WrappedComparable<Guid>)(object)nr.AppId == (WrappedComparable<Guid>)(object)appId
			select nr;
		return source.Any();
	}

	public IPermissionSet GetPermissionSet(int companyNameToken, NavApplicationObjectBase callingObject, IEnumerable<ApplicationObjectId> applicationObjects)
	{
		if (applicationObjects == null)
		{
			throw new ArgumentNullException("applicationObjects");
		}
		return GetPermissionManager(companyNameToken).GetPermissionSet(callingObject, applicationObjects);
	}

	public IPermissionSet GetPermissionSet(int companyNameToken, NavApplicationObjectBase callingObject, ApplicationObjectId applicationObject)
	{
		return GetPermissionManager(companyNameToken).GetPermissionSet(callingObject, applicationObject);
	}

	internal FilterFieldDictionary GetSecurityFilters(int companyToken, int tableId, SecurityFilterType securityFilterType, NavApplicationObjectBase callingObject, NavApplicationObjectBase securableObject)
	{
		if (IsSuperForAllCompanies)
		{
			return FilterFieldDictionary.Empty;
		}
		PermissionManager permissionManager = GetPermissionManager(companyToken);
		return permissionManager.GetSecurityFilters(tableId, securityFilterType, callingObject, securableObject);
	}

	public void MaximizePermissions(int[] tableObjects)
	{
		if (tableObjects == null)
		{
			return;
		}
		foreach (int key in tableObjects)
		{
			if (!maximizedPermissions.ContainsKey(key))
			{
				maximizedPermissions[key] = 1;
			}
			else
			{
				maximizedPermissions[key]++;
			}
		}
	}

	public void RemoveMaximizedPermissions(int[] tableObjects)
	{
		if (tableObjects == null)
		{
			return;
		}
		foreach (int key in tableObjects)
		{
			if (maximizedPermissions.ContainsKey(key))
			{
				maximizedPermissions[key]--;
				if (maximizedPermissions[key] == 0)
				{
					maximizedPermissions.Remove(key);
				}
			}
		}
	}

	public bool HasMaximizedPermissions(int tableId)
	{
		return maximizedPermissions.ContainsKey(tableId);
	}

	public bool IsSuperInCurrentCompany(int companyToken)
	{
		return GetRoles(companyToken).Any((NavRole r) => r.Id == "SUPER");
	}

	private HashSet<NavRole> FetchRoles(int companyNameToken)
	{
		HashSet<NavRole> hashSet;
		using (new MaximizedPermissionScope(session, new int[1] { 2000000053 }))
		{
			hashSet = FetchRolesFromId(user.Id, companyNameToken);
			if (user.Groups != null)
			{
				foreach (IdentityReference group in user.Groups)
				{
					if (session.Tenant.Database.NavUsers.TryGetWindowsGroupFromSid(group.Value, out var navUser))
					{
						hashSet.UnionWith(FetchRolesFromId(navUser.Id, companyNameToken));
					}
				}
			}
		}
		return hashSet;
	}

	private HashSet<NavRole> FetchRolesFromId(Guid userid, int companyNameToken)
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected Obj, but got Unknown
		HashSet<NavRole> hashSet = new HashSet<NavRole>();
		using (NavRecord navRecord = new NavRecord(session, 2000000053, SecurityFiltering.Ignored))
		{
			navRecord.SetFilter(navRecord.MetaTable.GetFieldByNo(1), "%1", new NavGuid(userid));
			if (!navRecord.ALFind(DataError.TrapError, "-"))
			{
				return hashSet;
			}
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(8);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(9);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(2);
			NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(3);
			string companyNameFromToken = session.Database.GetCompanyNameFromToken(companyNameToken);
			do
			{
				string text = navRecord.GetFieldValue(fieldByNo4).ToString();
				if (string.IsNullOrEmpty(text) || session.Database.CompanyNameComparer.Equals(text, companyNameFromToken))
				{
					PermissionScope permissionScope = (PermissionScope)navRecord.GetFieldValue(fieldByNo).ToInt32();
					AppId val = new AppId(navRecord.GetFieldValue(fieldByNo2).ToGuid());
					string text2 = navRecord.GetFieldValue(fieldByNo3).ToString();
					hashSet.Add(session.Database.SecurityAndLicense.GetRole(text2, permissionScope, val));
					if (string.IsNullOrEmpty(text) && text2.Equals("SUPER", StringComparison.Ordinal) && permissionScope == PermissionScope.System && (WrappedComparable<Guid>)(object)val == (WrappedComparable<Guid>)(object)AppId.None)
					{
						isSuperUserForAllCompanies = true;
					}
				}
			}
			while (navRecord.NextRecord());
		}
		if (!isSuperUserForAllCompanies.HasValue)
		{
			isSuperUserForAllCompanies = false;
		}
		return hashSet;
	}

	internal static HashSet<NavRole> FetchRolesFromRoleId(NavSession session, string roleId)
	{
		HashSet<NavRole> hashSet = new HashSet<NavRole>();
		hashSet.Add(session.Database.SecurityAndLicense.GetRole(roleId, PermissionScope.System, AppId.None));
		hashSet.Add(session.Database.SecurityAndLicense.GetRole(roleId, PermissionScope.Tenant, AppId.None));
		return hashSet;
	}

	internal static HashSet<NavRole> FetchRolesFromRoleIdAndAppId(NavSession session, string roleId, AppId appId)
	{
		HashSet<NavRole> hashSet = new HashSet<NavRole>();
		hashSet.Add(session.Database.SecurityAndLicense.GetRole(roleId, PermissionScope.Tenant, appId));
		return hashSet;
	}

	internal PermissionMask GetEffectivePermissionForObject(string companyName, ApplicationObjectId objectId)
	{
		int companyNameToken = session.Database.GetCompanyNameToken(companyName);
		return GetPermissionManager(companyNameToken).GetPermissions(user, objectId);
	}

	internal PermissionManager GetPermissionManager(int companyNameToken)
	{
		PermissionManager permissionManager = permissionManagers.Find((PermissionManager pm) => pm.CompanyNameToken == companyNameToken);
		if (permissionManager == null)
		{
			permissionManager = new PermissionManager(session, companyNameToken);
			permissionManagers.Add(permissionManager);
			if (FeatureSwitches.Instance.EnableSecurityTracing)
			{
				session.Diagnostics.SendTraceTag("00000YF", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Create PermissionManager({0})", companyNameToken);
			}
		}
		return permissionManager;
	}

	private void EnsurePermissionSetupVersionCorrect()
	{
		if (localSetupVersion != session.Database.PermissionSetupMonitor.SetupVersion)
		{
			if (FeatureSwitches.Instance.EnableSecurityTracing)
			{
				session.Diagnostics.SendTraceTag("00000YG", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "EnsurePermissionSetupVersionCorrect");
			}
			roles.Clear();
			localSetupVersion = session.Database.PermissionSetupMonitor.SetupVersion;
			isSuperUserForAllCompanies = null;
		}
	}
}
