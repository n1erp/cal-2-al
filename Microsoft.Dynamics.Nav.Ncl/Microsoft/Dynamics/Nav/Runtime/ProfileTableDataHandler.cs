using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class ProfileTableDataHandler
{
	private static readonly NCLMetaTable AllProfile = NavGlobal.NCLMetadata.GetMetaTableById(2000000178, 0);

	public static IEnumerable<ReadOnlyRecordBuffer> GetAllItems(bool IsTenantProfileDefault = false)
	{
		using NavRecord profileRecord = new NavRecord(NavCurrentThread.Session, 2000000072);
		int profileIdLength = AllProfile.GetFieldByNo(3).FieldDefinedLength;
		int descriptionLength = AllProfile.GetFieldByNo(4).FieldDefinedLength;
		int recordNotebookLength = AllProfile.GetFieldByNo(10).FieldDefinedLength;
		int pageNotebookLength = AllProfile.GetFieldByNo(12).FieldDefinedLength;
		NCLMetaField scopeField = AllProfile.GetFieldByNo(1);
		bool flag = profileRecord.ALFindFirst(DataError.TrapError);
		while (flag)
		{
			yield return new ReadOnlyRecordBuffer(AllProfile, VirtualDataProvider.VirtualTimeStamp, NavOption.Create(scopeField.FieldOptionMetadata, 0), NavGuid.Default, NavCode.CreateTruncated(profileIdLength, profileRecord.GetFieldValue(3).ToString()), NavText.CreateTruncated(descriptionLength, profileRecord.GetFieldValue(12).ToString()), NavInteger.Create(profileRecord.GetFieldValue(15).ToInt32()), NavBoolean.Create(profileRecord.GetFieldValue(18).ToBoolean() && !IsTenantProfileDefault), NavBoolean.Create(profileRecord.GetFieldValue(21).ToBoolean()), NavBoolean.Create(profileRecord.GetFieldValue(24).ToBoolean()), NavBoolean.Create(profileRecord.GetFieldValue(27).ToBoolean()), NavText.CreateTruncated(recordNotebookLength, profileRecord.GetFieldValue(30).ToString()), NavBoolean.Create(profileRecord.GetFieldValue(33).ToBoolean()), NavText.CreateTruncated(pageNotebookLength, profileRecord.GetFieldValue(36).ToString()), NavBoolean.Create(profileRecord.GetFieldValue(39).ToBoolean()), NavText.Empty);
			flag = profileRecord.NextRecord();
		}
	}

	public static InsertResult Insert(MutableRecordBuffer recordBuffer)
	{
		NavSession session = NavCurrentThread.Session;
		NavRecord record = new NavRecord(session, 2000000072);
		try
		{
			NavValue navValue = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)];
			if (record.ALGet(DataError.TrapError, navValue))
			{
				string arg = ProfileScope.System.ToString();
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ProfileAlreadyExists, navValue, arg));
			}
			record.Clear();
			record.SetFieldValue(3, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)]);
			record.SetFieldValue(12, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(4)]);
			record.SetFieldValue(15, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(5)]);
			record.SetFieldValue(18, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(6)]);
			record.SetFieldValue(21, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(7)]);
			record.SetFieldValue(24, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(8)]);
			record.SetFieldValue(27, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(9)]);
			record.SetFieldValue(30, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(10)]);
			record.SetFieldValue(33, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(11)]);
			record.SetFieldValue(36, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(12)]);
			record.SetFieldValue(39, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(13)]);
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
		NavRecord record = NavCurrentThread.Session.GetGlobalRecordInstance(2000000072);
		try
		{
			if (!recordBuffer.IsChangedValueSameAsOriginalValue(3))
			{
				NavValue navValue = recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)];
				if (record.ALGet(DataError.TrapError, navValue))
				{
					string arg = ProfileScope.System.ToString();
					throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ProfileAlreadyExists, navValue, arg));
				}
				record.Clear();
				record.ALGet(recordBuffer.ReadOnlyBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)]);
				session.TransactionScope(() =>
				{
					record.ALRename(recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)]);
				});
			}
			record.ALGet(recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)]);
			if (!recordBuffer.IsChangedValueSameAsOriginalValue(6))
			{
				record.SetFieldValue(18, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(6)]);
			}
			record.SetFieldValue(12, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(4)]);
			record.SetFieldValue(15, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(5)]);
			record.SetFieldValue(21, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(7)]);
			record.SetFieldValue(24, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(8)]);
			record.SetFieldValue(27, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(9)]);
			record.SetFieldValue(30, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(10)]);
			record.SetFieldValue(33, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(11)]);
			record.SetFieldValue(36, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(12)]);
			record.SetFieldValue(39, recordBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(13)]);
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
		NavRecord record = NavCurrentThread.Session.GetGlobalRecordInstance(2000000072);
		try
		{
			record.ALGet(recordBuffer.ReadOnlyBuffer[recordBuffer.MetaTable.GetFieldIndexByNo(3)]);
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
