using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class LimitedUserPermissionChecker
{
	private const int MaxNumberOfOptionalTablesWithWriteAccess = 3;

	private readonly int[] optionalTablesWithWriteAccess = new int[3];

	private int numberOfUsedOptionalTables;

	internal bool CheckPermissionsForTableData(PermissionSetBase tablePermissionSet, NavApplicationObjectBase callingObject, PermissionMask permissions, Func<int, bool> checkTableEmpty, PermissionMask additionalIndirectPermissions, bool countAsLimitedUserAllowedTable)
	{
		if (tablePermissionSet.HasMaximizedPermissions)
		{
			PermissionSetBase.LogPermissionCheck(hasPermissions: true, tablePermissionSet.ObjectId, NavCurrentThread.Session, permissions, callingObject);
			return true;
		}
		if (!tablePermissionSet.InternalHasPermissionsDoNotCheckIncludedExcludedPermissions(callingObject, permissions, null, additionalIndirectPermissions, checkMaximizedPermissions: false))
		{
			bool flag = CheckIfTableIsEmpty(tablePermissionSet.ObjectId.ObjectNumber, checkTableEmpty);
			PermissionSetBase.LogPermissionCheck(flag, tablePermissionSet.ObjectId, NavCurrentThread.Session, permissions, callingObject);
			return flag;
		}
		if (((PermissionMask.Insert | PermissionMask.Modify | PermissionMask.Delete | PermissionMask.IndirectInsert | PermissionMask.IndirectModify | PermissionMask.IndirectDelete) & permissions) == 0)
		{
			PermissionSetBase.LogPermissionCheck(hasPermissions: true, tablePermissionSet.ObjectId, NavCurrentThread.Session, permissions, callingObject);
			return true;
		}
		if (tablePermissionSet.ActualDirectPermissions.HasFlag(PermissionMask.IncludedLimitedUsage))
		{
			PermissionSetBase.LogPermissionCheck(hasPermissions: true, tablePermissionSet.ObjectId, NavCurrentThread.Session, permissions, callingObject);
			return true;
		}
		if (tablePermissionSet.ActualDirectPermissions.HasFlag(PermissionMask.ExcludedLimitedUsage))
		{
			bool flag2 = CheckIfTableIsEmpty(tablePermissionSet.ObjectId.ObjectNumber, checkTableEmpty);
			PermissionSetBase.LogPermissionCheck(flag2, tablePermissionSet.ObjectId, NavCurrentThread.Session, PermissionMask.Modify, callingObject);
			return flag2;
		}
		return CheckOptionalWritePermissionForTableData(tablePermissionSet.ObjectId.ObjectNumber, checkTableEmpty, countAsLimitedUserAllowedTable);
	}

	protected virtual bool CheckOptionalWritePermissionForTableData(int tableNo, Func<int, bool> checkTableEmpty, bool countAsLimitedUserAllowedTable)
	{
		if (Array.Find(optionalTablesWithWriteAccess, (int no) => no == tableNo) == tableNo)
		{
			return true;
		}
		bool flag = CheckIfTableIsEmpty(tableNo, checkTableEmpty);
		if (numberOfUsedOptionalTables == 3)
		{
			if (!flag)
			{
				LogErrorIntoEventLog(tableNo);
			}
			return flag;
		}
		if (flag)
		{
			return true;
		}
		if (countAsLimitedUserAllowedTable)
		{
			optionalTablesWithWriteAccess[numberOfUsedOptionalTables++] = tableNo;
		}
		return true;
	}

	private void LogErrorIntoEventLog(int tableNo)
	{
		string tableNameByTableId = GetTableNameByTableId(optionalTablesWithWriteAccess[0]);
		string tableNameByTableId2 = GetTableNameByTableId(optionalTablesWithWriteAccess[1]);
		string tableNameByTableId3 = GetTableNameByTableId(optionalTablesWithWriteAccess[2]);
		string tableNameByTableId4 = GetTableNameByTableId(tableNo);
		NavServerEventSource.Log.LimitedUserOptionalTablePermissionErrorInfoMessage(NavEnvironment.ServiceInstanceName, NavEnvironment.CurrentTenantId, string.Format(CultureInfo.CurrentCulture, Lang.LimitedUserOptionalTablePermissionErrorLogMessage, tableNameByTableId4, 3, tableNameByTableId, tableNameByTableId2, tableNameByTableId3));
	}

	protected virtual string GetTableNameByTableId(int tableId)
	{
		return NavGlobal.NCLMetadata.GetTableNameByTableId(tableId);
	}

	private static bool CheckIfTableIsEmpty(int tableNo, Func<int, bool> checkTableEmpty)
	{
		return checkTableEmpty?.Invoke(tableNo) ?? false;
	}

	internal virtual int NumberOfOptionalAllowedToWriteTables()
	{
		return numberOfUsedOptionalTables;
	}
}
