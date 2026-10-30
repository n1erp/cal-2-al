using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class AggregatePermissionSetDataProvider : EagerVirtualDataProvider
{
	private const int PrimaryKeyScopeIndex = 0;

	private const int PrimaryKeyAppIdIndex = 1;

	private const int PrimaryKeyRoleIdIndex = 2;

	private readonly NCLMetaField roleIdField;

	private readonly NCLMetaField nameField;

	private readonly NCLMetaField scopeField;

	private readonly NCLMetaField appNameField;

	private readonly NCLMetaField appIdField;

	public override int TableId => 2000000167;

	public AggregatePermissionSetDataProvider()
	{
		roleIdField = MetaTable.GetFieldByNo(3);
		nameField = MetaTable.GetFieldByNo(4);
		scopeField = MetaTable.GetFieldByNo(1);
		appNameField = MetaTable.GetFieldByNo(5);
		appIdField = MetaTable.GetFieldByNo(2);
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = true;
		return GetSystemPermissionSets().Concat(GetTenantPermissionSets());
	}

	public override bool TryGetByPrimaryKey(RecordIdProviderRequest request, out ReadOnlyRecordBuffer recordBuffer)
	{
		NavRecordId recordId = request.RecordId;
		IEnumerable<ReadOnlyRecordBuffer> source = Enumerable.Empty<ReadOnlyRecordBuffer>();
		NavValue roleId = recordId.FieldValueByIndex(roleIdField, 2);
		switch (recordId.FieldValueByIndex(scopeField, 0).ToInt32())
		{
		case 0:
			source = GetSystemPermissionSets(roleId);
			break;
		case 1:
		{
			NavValue appid = recordId.FieldValueByIndex(appIdField, 1);
			source = GetTenantPermissionSets(appid, roleId);
			break;
		}
		}
		ReadOnlyRecordBuffer readOnlyRecordBuffer = source.FirstOrDefault();
		if (readOnlyRecordBuffer != null && readOnlyRecordBuffer.PassesFieldFiltersAndMarks(request.FiltersAndMarks))
		{
			recordBuffer = readOnlyRecordBuffer;
			return true;
		}
		recordBuffer = null;
		return false;
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetSystemPermissionSets(NavValue roleId = null)
	{
		using NavRecord psRecord = new NavRecord(NavCurrentThread.Session, 2000000004);
		NCLMetaTable nCLMetaTable = psRecord.MetaTable;
		NCLMetaField timeStampField = nCLMetaTable.GetFieldByNo(0);
		NCLMetaField roleIdField = nCLMetaTable.GetFieldByNo(1);
		NCLMetaField nameField = nCLMetaTable.GetFieldByNo(2);
		bool findAll = roleId == null;
		bool flag = (findAll ? psRecord.ALFindSet(DataError.TrapError) : psRecord.ALGet(DataError.TrapError, roleId));
		while (flag)
		{
			yield return new ReadOnlyRecordBuffer(MetaTable, psRecord.GetFieldValue(timeStampField), NavOption.Create(scopeField.FieldOptionMetadata, 0), NavGuid.Default, NavCode.CreateTruncated(this.roleIdField.FieldDefinedLength, psRecord.GetFieldValue(roleIdField).ToString()), NavText.CreateTruncated(this.nameField.FieldDefinedLength, psRecord.GetFieldValue(nameField).ToString()), NavText.Empty);
			flag = findAll && psRecord.NextRecord();
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetTenantPermissionSets(NavValue appid = null, NavValue roleId = null)
	{
		NavAppGroup navAppGroup = NavCurrentThread.ResolveAppGroup();
		Dictionary<Guid, string> appNames = navAppGroup.OrderedAppMetadata.ToDictionary((NavAppRuntimeMetadata m) => ((WrappedValue<Guid>)(object)m.AppId).Value, (NavAppRuntimeMetadata m) => m.Name);
		using NavRecord tpsRecord = new NavRecord(NavCurrentThread.Session, 2000000165);
		NCLMetaTable nCLMetaTable = tpsRecord.MetaTable;
		NCLMetaField timeStampField = nCLMetaTable.GetFieldByNo(0);
		NCLMetaField appIdField = nCLMetaTable.GetFieldByNo(1);
		NCLMetaField roleIdField = nCLMetaTable.GetFieldByNo(2);
		NCLMetaField nameField = nCLMetaTable.GetFieldByNo(3);
		bool findAll = roleId == null || appid == null;
		bool flag = (findAll ? tpsRecord.ALFindSet(DataError.TrapError) : tpsRecord.ALGet(DataError.TrapError, appid, roleId));
		while (flag)
		{
			NavValue fieldValue = tpsRecord.GetFieldValue(appIdField);
			Guid key = fieldValue.ToGuid();
			string value = string.Empty;
			appNames.TryGetValue(key, out value);
			yield return new ReadOnlyRecordBuffer(MetaTable, tpsRecord.GetFieldValue(timeStampField), NavOption.Create(scopeField.FieldOptionMetadata, 1), fieldValue, NavCode.CreateTruncated(this.roleIdField.FieldDefinedLength, tpsRecord.GetFieldValue(roleIdField).ToString()), NavText.CreateTruncated(this.nameField.FieldDefinedLength, tpsRecord.GetFieldValue(nameField).ToString()), NavText.CreateTruncated(appNameField.FieldDefinedLength, value));
			flag = findAll && tpsRecord.NextRecord();
		}
	}
}
