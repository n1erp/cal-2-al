using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class TenantProfileTableDataHandler
{
	private static readonly NCLMetaTable AllProfile = NavGlobal.NCLMetadata.GetMetaTableById(2000000178, 0);

	public static bool IsTenantProfileDefault;

	public static IEnumerable<ReadOnlyRecordBuffer> GetAllItems()
	{
		IsTenantProfileDefault = false;
		NavAppGroup navAppGroup = NavCurrentThread.ResolveAppGroup();
		Dictionary<Guid, string> appNames = navAppGroup.OrderedAppMetadata.ToDictionary((NavAppRuntimeMetadata m) => ((WrappedValue<Guid>)(object)m.AppId).Value, (NavAppRuntimeMetadata m) => m.Name);
		using NavRecord tenantProfileRecord = new NavRecord(NavCurrentThread.Session, 2000000177);
		int profileIdLength = AllProfile.GetFieldByNo(3).FieldDefinedLength;
		int descriptionLength = AllProfile.GetFieldByNo(4).FieldDefinedLength;
		int appNameLength = AllProfile.GetFieldByNo(14).FieldDefinedLength;
		NCLMetaField scopeField = AllProfile.GetFieldByNo(1);
		bool flag = tenantProfileRecord.ALFindFirst(DataError.TrapError);
		while (flag)
		{
			NavValue fieldValue = tenantProfileRecord.GetFieldValue(1);
			Guid key = fieldValue.ToGuid();
			string value = string.Empty;
			appNames.TryGetValue(key, out value);
			bool flag2 = tenantProfileRecord.GetFieldValue(5).ToBoolean();
			if (flag2)
			{
				IsTenantProfileDefault = true;
			}
			yield return new ReadOnlyRecordBuffer(AllProfile, VirtualDataProvider.VirtualTimeStamp, NavOption.Create(scopeField.FieldOptionMetadata, 1), fieldValue, NavCode.CreateTruncated(profileIdLength, tenantProfileRecord.GetFieldValue(2).ToString()), NavText.CreateTruncated(descriptionLength, tenantProfileRecord.GetFieldValue(3).ToString()), NavInteger.Create(tenantProfileRecord.GetFieldValue(4).ToInt32()), NavBoolean.Create(flag2), NavBoolean.Create(value: false), NavBoolean.Create(value: false), NavBoolean.Create(value: false), NavText.Empty, NavBoolean.Create(value: false), NavText.Empty, NavBoolean.Create(tenantProfileRecord.GetFieldValue(6).ToBoolean()), NavText.CreateTruncated(appNameLength, value));
			flag = tenantProfileRecord.NextRecord();
		}
	}

	public static InsertResult Insert(MutableRecordBuffer recordBuffer)
	{
		NavSession session = NavCurrentThread.Session;
		NavRecord record = new NavRecord(session, 2000000177);
		try
		{
			NavValue navValue = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)];
			NavValue navValue2 = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(2)];
			if (record.ALGet(DataError.TrapError, navValue2, navValue))
			{
				string arg = ProfileScope.Tenant.ToString();
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ProfileAlreadyExists, navValue, arg));
			}
			record.Clear();
			record.SetFieldValue(1, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(2)]);
			record.SetFieldValue(2, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)]);
			record.SetFieldValue(3, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(4)]);
			record.SetFieldValue(4, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(5)]);
			record.SetFieldValue(5, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(6)]);
			record.SetFieldValue(6, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(13)]);
			session.TransactionScope(() =>
			{
				record.ALInsert(runApplicationTrigger: true);
			});
		}
		finally
		{
			if (record != null)
			{
				((IDisposable)record).Dispose();
			}
		}
		return InsertResult.Success;
	}

	public static ModifyResult Modify(MutableRecordBuffer recordBuffer)
	{
		NavSession session = NavCurrentThread.Session;
		NavValue navValue = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)];
		NavRecord record = NavCurrentThread.Session.GetGlobalRecordInstance(2000000177);
		try
		{
			if (recordBuffer.ReadOnlyBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(2)].ToGuid() != Guid.Empty && (!recordBuffer.IsChangedValueSameAsOriginalValue(2) || !recordBuffer.IsChangedValueSameAsOriginalValue(3) || !recordBuffer.IsChangedValueSameAsOriginalValue(5)))
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ModifySpecificFieldsOnAppProfileNotAllowed, navValue));
			}
			if (!recordBuffer.IsChangedValueSameAsOriginalValue(3) || !recordBuffer.IsChangedValueSameAsOriginalValue(2))
			{
				NavValue navValue2 = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(2)];
				if (record.ALGet(DataError.TrapError, navValue2, navValue))
				{
					string arg = ProfileScope.Tenant.ToString();
					throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ProfileAlreadyExists, navValue, arg));
				}
				record.Clear();
				record.ALGet(recordBuffer.ReadOnlyBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(2)], recordBuffer.ReadOnlyBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)]);
				session.TransactionScope(() =>
				{
					record.ALRename(recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(2)], recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)]);
				});
			}
			record.ALGet(recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(2)], recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)]);
			record.SetFieldValue(3, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(4)]);
			record.SetFieldValue(4, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(5)]);
			record.SetFieldValue(5, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(6)]);
			record.SetFieldValue(6, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(13)]);
			session.TransactionScope(() =>
			{
				record.ALModify(runApplicationTrigger: true);
			});
		}
		finally
		{
			if (record != null)
			{
				((IDisposable)record).Dispose();
			}
		}
		return ModifyResult.Success;
	}

	public static DeleteResult Delete(MutableRecordBuffer recordBuffer)
	{
		NavSession session = NavCurrentThread.Session;
		NavRecord record = NavCurrentThread.Session.GetGlobalRecordInstance(2000000177);
		try
		{
			if (recordBuffer.ReadOnlyBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(2)].ToGuid() != Guid.Empty)
			{
				string arg = recordBuffer.ReadOnlyBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)].ToString();
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.DeleteAppProfileNotAllowed, arg));
			}
			record.ALGet(recordBuffer.ReadOnlyBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(2)], recordBuffer.ReadOnlyBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)]);
			session.TransactionScope(() =>
			{
				record.ALDelete(runApplicationTrigger: true);
			});
		}
		finally
		{
			if (record != null)
			{
				((IDisposable)record).Dispose();
			}
		}
		return DeleteResult.Success;
	}
}
