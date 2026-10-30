using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavDatabaseSecurityAndLicense : IDisposable
{
	private readonly ConcurrentDictionary<NavRoleKey, NavRole> roles;

	private bool? permissionSystemEnabled;

	private readonly ReaderWriterLockSlim licenseLock = new ReaderWriterLockSlim();

	private readonly PermissionCache licensePermissions;

	private bool? securityFiltersPresent;

	private readonly Func<ApplicationObjectId, PermissionMask> getLicenseFunc;

	private INavLicense license;

	private readonly NavDatabase parentDatabase;

	private bool isDisposed;

	private const int CloudAlRangeStart = 70000000;

	private const int CloudAlRangeEnd = 75000000;

	internal const int FreePublishingRangeStart = 50000;

	internal const int FreePublishingRangeEnd = 99000;

	public bool IsSecurityFiltersActive
	{
		get
		{
			bool? flag = securityFiltersPresent;
			if (flag.HasValue)
			{
				return flag.Value;
			}
			if (parentDatabase.DatabaseType != NavDatabaseType.Tenant)
			{
				securityFiltersPresent = false;
				return securityFiltersPresent.Value;
			}
			securityFiltersPresent = AreSystemSecurityFiltersActive() || AreTenantSecurityFiltersActive();
			return securityFiltersPresent.Value;
		}
	}

	internal INavLicense License
	{
		get
		{
			INavLicense navLicense = license;
			if (navLicense != null)
			{
				return navLicense;
			}
			navLicense = ((parentDatabase.DatabaseType != NavDatabaseType.Tenant || parentDatabase.TenantProperties.License == null) ? NavLicense.Create(NavGlobal.AppDatabase.SqlDatabaseProperties.CheckLicenseExists(), NavSession.LicenseExpiryHandler) : NavLicense.Create(parentDatabase.TenantProperties.License, NavSession.LicenseExpiryHandler));
			INavLicense navLicense2 = Interlocked.CompareExchange(ref license, navLicense, null);
			if (navLicense2 != null)
			{
				navLicense.Dispose();
				return navLicense2;
			}
			return navLicense;
		}
	}

	public NavDatabaseSecurityAndLicense(NavDatabase parentDatabase)
	{
		this.parentDatabase = parentDatabase;
		getLicenseFunc = (ApplicationObjectId objectId) => NavRecord.IsTableVirtual(objectId.ObjectNumber) ? PermissionMask.Max : NavCurrentThread.Session.License.GetLicensePermissionMask(objectId.ObjectType, objectId.ObjectNumber);
		roles = new ConcurrentDictionary<NavRoleKey, NavRole>();
		licensePermissions = new PermissionCache();
	}

	private static bool AreSystemSecurityFiltersActive()
	{
		bool result = false;
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000053 }))
		{
			using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000005 }))
			{
				using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000053, SecurityFiltering.Ignored);
				using NavRecord navRecord2 = new NavRecord(NavCurrentThread.Session, 2000000005, SecurityFiltering.Ignored);
				NCLMetaField fieldByNo = navRecord2.MetaTable.GetFieldByNo(3);
				NCLMetaField fieldByNo2 = navRecord2.MetaTable.GetFieldByNo(11);
				NCLMetaField fieldByNo3 = navRecord2.MetaTable.GetFieldByNo(1);
				navRecord2.SetFilter(fieldByNo, "=%1", NavOption.Create(fieldByNo.FieldOptionMetadata, 0));
				navRecord2.SetFilter(fieldByNo2, "<>%1", NavTableFilter.Default);
				if (!navRecord2.ALIsEmpty)
				{
					NCLOptionMetadata fieldOptionMetadata = navRecord.MetaTable.GetFieldByNo(8).FieldOptionMetadata;
					navRecord.ALSetRange(8, NavOption.Create(fieldOptionMetadata, 0));
					if (navRecord.ALFindSet(DataError.TrapError))
					{
						do
						{
							NavValue fieldValue = navRecord.GetFieldValue(2);
							navRecord2.SetFilter(fieldByNo3, "=%1", fieldValue);
							if (!navRecord2.ALIsEmpty)
							{
								result = true;
								break;
							}
						}
						while (navRecord.ALNext() != 0);
					}
				}
			}
		}
		return result;
	}

	private static bool AreTenantSecurityFiltersActive()
	{
		bool result = false;
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000053 }))
		{
			using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000166 }))
			{
				using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000053, SecurityFiltering.Ignored);
				using NavRecord navRecord2 = new NavRecord(NavCurrentThread.Session, 2000000166, SecurityFiltering.Ignored);
				NCLMetaField fieldByNo = navRecord2.MetaTable.GetFieldByNo(4);
				NCLMetaField fieldByNo2 = navRecord2.MetaTable.GetFieldByNo(12);
				navRecord2.SetFilter(fieldByNo, "=%1", NavOption.Create(fieldByNo.FieldOptionMetadata, 0));
				navRecord2.SetFilter(fieldByNo2, "<>%1", NavTableFilter.Default);
				if (!navRecord2.ALIsEmpty)
				{
					NCLOptionMetadata fieldOptionMetadata = navRecord.MetaTable.GetFieldByNo(8).FieldOptionMetadata;
					navRecord.ALSetRange(8, NavOption.Create(fieldOptionMetadata, 1));
					if (navRecord.ALFindSet(DataError.TrapError))
					{
						do
						{
							NavValue fieldValue = navRecord.GetFieldValue(2);
							NavValue fieldValue2 = navRecord.GetFieldValue(9);
							navRecord2.ALSetRange(2, fieldValue);
							navRecord2.ALSetRange(1, fieldValue2);
							if (!navRecord2.ALIsEmpty)
							{
								result = true;
								break;
							}
						}
						while (navRecord.ALNext() != 0);
					}
				}
			}
		}
		return result;
	}

	public PermissionMask GetLicensePermissions(ApplicationObjectId objectId)
	{
		if (IsPermissionAllowedOutsideLicense(objectId, out var permission))
		{
			return permission;
		}
		licenseLock.EnterReadLock();
		try
		{
			if (licensePermissions.TryGetValue(objectId, out permission))
			{
				return permission;
			}
		}
		finally
		{
			licenseLock.ExitReadLock();
		}
		permission = getLicenseFunc(objectId);
		if (IsCalObjectInAlCloudObjectRange(objectId))
		{
			permission = PermissionMask.None;
		}
		else if ((permission & PermissionMask.Read) == PermissionMask.Read)
		{
			permission |= PermissionMask.Execute;
		}
		licenseLock.EnterUpgradeableReadLock();
		try
		{
			if (licensePermissions.TryGetValue(objectId, out var result))
			{
				return result;
			}
			licenseLock.EnterWriteLock();
			try
			{
				licensePermissions.AddOrSet(objectId, permission);
				return permission;
			}
			finally
			{
				licenseLock.ExitWriteLock();
			}
		}
		finally
		{
			licenseLock.ExitUpgradeableReadLock();
		}
	}

	public bool IsPermissionSystemEnabled()
	{
		if (NavTestExecution.UseEffectivePermissions())
		{
			return true;
		}
		bool? flag = permissionSystemEnabled;
		if (flag.HasValue)
		{
			return flag.Value;
		}
		if (parentDatabase.DatabaseType != NavDatabaseType.Tenant)
		{
			permissionSystemEnabled = false;
			return permissionSystemEnabled.Value;
		}
		bool flag2;
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000053 }))
		{
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000053, SecurityFiltering.Ignored);
			flag2 = !navRecord.ALIsEmpty;
		}
		permissionSystemEnabled = flag2;
		return flag2;
	}

	public void ClearPermissions(bool clearLicenseInfo)
	{
		permissionSystemEnabled = null;
		roles.Clear();
		securityFiltersPresent = null;
		if (!clearLicenseInfo)
		{
			return;
		}
		licenseLock.EnterWriteLock();
		try
		{
			licensePermissions.Clear();
			if (license != null)
			{
				license.Dispose();
				license = null;
			}
			NavGlobal.AppDatabase.SqlDatabaseProperties.MarkAsNotReady();
			if (parentDatabase.DatabaseType == NavDatabaseType.Tenant)
			{
				parentDatabase.TenantDatabaseProperties.MarkAsNotReady();
				parentDatabase.TenantProperties.MarkAsNotReady();
			}
		}
		finally
		{
			licenseLock.ExitWriteLock();
		}
	}

	public NavRole GetRole(string roleId, PermissionScope scope, AppId appId)
	{
		NavRoleKey key = new NavRoleKey(roleId, scope, appId);
		return roles.GetOrAdd(key, (NavRoleKey roleKey) => new NavRole(roleKey));
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!isDisposed & disposing)
		{
			if (licenseLock != null)
			{
				licenseLock.Dispose();
			}
			if (license != null)
			{
				license.Dispose();
				license = null;
			}
			isDisposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private static bool IsPermissionAllowedOutsideLicense(ApplicationObjectId objectId, out PermissionMask permission)
	{
		permission = PermissionMask.None;
		if (objectId.ObjectType == ObjectType.System && objectId.ObjectNumber == 1350)
		{
			DiagnosticsResolver.GetMostSpecificInstance()?.SendTraceTag("00001U0", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "A user has used the RunTable permission to view the contents of a table.");
			permission = PermissionMask.Execute;
			return true;
		}
		return false;
	}

	private static bool IsCalObjectInAlCloudObjectRange(ApplicationObjectId objectId)
	{
		if (objectId.ObjectNumber < 70000000 || objectId.ObjectNumber > 75000000)
		{
			return false;
		}
		ObjectType objectType = objectId.ObjectType;
		if (objectType == ObjectType.TableData)
		{
			objectType = ObjectType.Table;
		}
		NavAppGroup navAppGroup = NavCurrentThread.ResolveAppGroup();
		return navAppGroup.GetObjectOwner(objectType, objectId.ObjectNumber) == null;
	}
}
