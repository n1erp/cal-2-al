using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavRole
{
	private PermissionCache permissions;

	private readonly object permissionLock = new object();

	private string name;

	public string Id => Key.Id;

	public PermissionScope Scope => Key.Scope;

	public AppId AppId => Key.AppId;

	public NavRoleKey Key { get; private set; }

	public string Name
	{
		get
		{
			if (string.IsNullOrEmpty(name))
			{
				ReadName();
			}
			return name;
		}
	}

	public PermissionCache Permissions
	{
		get
		{
			PermissionCache permissionCache;
			lock (permissionLock)
			{
				permissionCache = permissions;
			}
			if (permissionCache == null)
			{
				permissionCache = ((Scope != PermissionScope.System) ? ReadTenantPermissions() : ReadSystemPermissions());
			}
			lock (permissionLock)
			{
				if (permissions == null)
				{
					permissions = permissionCache;
				}
			}
			return permissions;
		}
	}

	public NavRole(NavRoleKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		Key = key;
	}

	private void ReadName()
	{
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000167 }))
		{
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000167, SecurityFiltering.Ignored);
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(1);
			navRecord.ALGet(NavOption.Create(fieldByNo.FieldOptionMetadata, (int)Scope), new NavGuid(((WrappedValue<Guid>)(object)AppId).Value), new NavText(Id.Length, Id));
			name = navRecord.GetFieldValue(4).ToString();
		}
	}

	private PermissionCache ReadSystemPermissions()
	{
		PermissionCache permissionCache = new PermissionCache();
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000005 }))
		{
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000005, SecurityFiltering.Ignored);
			navRecord.SetFilter(navRecord.MetaTable.GetFieldByNo(1), string.Format(CultureInfo.CurrentCulture, "='{0}'", Id));
			if (!navRecord.ALFind(DataError.TrapError, "-"))
			{
				return permissionCache;
			}
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(4);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(6);
			NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(7);
			NCLMetaField fieldByNo5 = navRecord.MetaTable.GetFieldByNo(8);
			NCLMetaField fieldByNo6 = navRecord.MetaTable.GetFieldByNo(9);
			NCLMetaField fieldByNo7 = navRecord.MetaTable.GetFieldByNo(10);
			do
			{
				PermissionMask permissionMask = PermissionMask.None;
				permissionMask |= GetPermissionMaskFromRecord(navRecord, fieldByNo3, PermissionMask.Read);
				permissionMask |= GetPermissionMaskFromRecord(navRecord, fieldByNo4, PermissionMask.Insert);
				permissionMask |= GetPermissionMaskFromRecord(navRecord, fieldByNo5, PermissionMask.Modify);
				permissionMask |= GetPermissionMaskFromRecord(navRecord, fieldByNo6, PermissionMask.Delete);
				permissionMask |= GetPermissionMaskFromRecord(navRecord, fieldByNo7, PermissionMask.Execute);
				ApplicationObjectId objectId = new ApplicationObjectId((ObjectType)navRecord.GetFieldValue(fieldByNo).ToInt32(), navRecord.GetFieldValue(fieldByNo2).ToInt32());
				permissionCache.AddOrSet(objectId, permissionCache.TryGetValue(objectId, out var result) ? (result | permissionMask) : permissionMask);
				if (objectId.ObjectType == ObjectType.TableData)
				{
					permissionCache.AddOrSetSecurityFilter(objectId.ObjectNumber, (NavTableFilter)navRecord.GetFieldValue(11));
				}
			}
			while (navRecord.NextRecord());
			return permissionCache;
		}
	}

	private PermissionCache ReadTenantPermissions()
	{
		PermissionCache permissionCache = new PermissionCache();
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000166 }))
		{
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000166, SecurityFiltering.Ignored);
			navRecord.SetFilter(navRecord.MetaTable.GetFieldByNo(1), "%1", new NavGuid(((WrappedValue<Guid>)(object)AppId).Value));
			navRecord.SetFilter(navRecord.MetaTable.GetFieldByNo(2), string.Format(CultureInfo.CurrentCulture, "='{0}'", Id.ToUpperInvariant()));
			if (!navRecord.ALFind(DataError.TrapError, "-"))
			{
				return permissionCache;
			}
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(4);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(5);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(7);
			NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(8);
			NCLMetaField fieldByNo5 = navRecord.MetaTable.GetFieldByNo(9);
			NCLMetaField fieldByNo6 = navRecord.MetaTable.GetFieldByNo(10);
			NCLMetaField fieldByNo7 = navRecord.MetaTable.GetFieldByNo(11);
			do
			{
				PermissionMask permissionMask = PermissionMask.None;
				permissionMask |= GetPermissionMaskFromRecord(navRecord, fieldByNo3, PermissionMask.Read);
				permissionMask |= GetPermissionMaskFromRecord(navRecord, fieldByNo4, PermissionMask.Insert);
				permissionMask |= GetPermissionMaskFromRecord(navRecord, fieldByNo5, PermissionMask.Modify);
				permissionMask |= GetPermissionMaskFromRecord(navRecord, fieldByNo6, PermissionMask.Delete);
				permissionMask |= GetPermissionMaskFromRecord(navRecord, fieldByNo7, PermissionMask.Execute);
				ApplicationObjectId objectId = new ApplicationObjectId((ObjectType)navRecord.GetFieldValue(fieldByNo).ToInt32(), navRecord.GetFieldValue(fieldByNo2).ToInt32());
				permissionCache.AddOrSet(objectId, permissionCache.TryGetValue(objectId, out var result) ? (result | permissionMask) : permissionMask);
				if (objectId.ObjectType == ObjectType.TableData)
				{
					permissionCache.AddOrSetSecurityFilter(objectId.ObjectNumber, (NavTableFilter)navRecord.GetFieldValue(12));
				}
			}
			while (navRecord.NextRecord());
			return permissionCache;
		}
	}

	private static PermissionMask GetPermissionMaskFromRecord(NavRecord permissionRecord, NCLMetaField permissionField, PermissionMask basePermissionMask)
	{
		return PermissionHelper.GetPermissionMaskFromPermissionOption((PermissionOption)permissionRecord.GetFieldValue(permissionField).ToInt32(), basePermissionMask);
	}
}
