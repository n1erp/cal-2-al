using System;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class RecordLink
{
	private const int MaxUrlFieldLength = 2048;

	public static int AddLink(NavRecord record, string url, string description)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (url == null)
		{
			throw new ArgumentNullException("url");
		}
		if (description == null)
		{
			throw new ArgumentNullException("description");
		}
		if (url.Length > 2048)
		{
			throw new NavInvalidLinkFormatException(Lang.RecordLinkUrlAboveMaxSize);
		}
		using NavRecord navRecord = new NavRecord(record, 2000000068);
		navRecord.SetFieldValue(2, record.ALRecordId);
		navRecord.SetFieldValue(7, new NavText(description.Length, description));
		navRecord.SetFieldValue(3, new NavText(url.Length, url));
		string text = record.Session.User.Name.Substring(record.Session.User.Name.IndexOf('\\') + 1);
		navRecord.SetFieldValue(11, new NavText(text.Length, text));
		navRecord.SetFieldValue(10, NavDateTime.Now);
		if (record.MetaTable.DataPerCompany)
		{
			navRecord.SetFieldValue(12, new NavText(record.CompanyName.Length, record.CompanyName));
		}
		navRecord.Insert(DataError.ThrowError, runApplicationTrigger: false, runGlobalTrigger: false);
		return navRecord.GetFieldValue(1).ToInt32();
	}

	public static void DeleteLink(NavRecord record, int linkId)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		using NavRecord navRecord = new NavRecord(record, 2000000068);
		navRecord.ALLockTable();
		if (navRecord.ALGet(DataError.TrapError, NavInteger.Create(linkId)) && navRecord.GetFieldValue(2).Equals(record.ALRecordId) && navRecord.GetFieldValue(12).ToString().Equals(record.CompanyName))
		{
			navRecord.Delete(DataError.ThrowError, runApplicationTrigger: false, isCalledFromUI: false);
		}
	}

	public static void CopyLinks(NavRecord sourceRecord, NavRecord destinationRecord)
	{
		if (sourceRecord == null)
		{
			throw new ArgumentNullException("sourceRecord");
		}
		if (destinationRecord == null)
		{
			throw new ArgumentNullException("destinationRecord");
		}
		int companyNameToken = sourceRecord.RecordImplementation.TableState.CompanyNameToken;
		int companyNameToken2 = destinationRecord.RecordImplementation.TableState.CompanyNameToken;
		NavRecordId aLRecordId = destinationRecord.ALRecordId;
		if ((companyNameToken == companyNameToken2 && sourceRecord.ALRecordId.Equals(aLRecordId)) || (!IsRecordLinkTableLocked(sourceRecord) && !TableHasLinks(sourceRecord, sourceRecord.MetaTable, sourceRecord.CompanyName)))
		{
			return;
		}
		using NavRecord navRecord = new NavRecord(sourceRecord, 2000000068);
		using NavRecord navRecord2 = new NavRecord(destinationRecord, 2000000068);
		SetupRecordLinkRanges(navRecord, sourceRecord);
		navRecord.ALLockTable();
		if (navRecord.ALFindFirst(DataError.TrapError))
		{
			do
			{
				navRecord.CalcFields(DataError.ThrowError, new int[1] { 9 });
				navRecord2.RecordImplementation.CopyRecord(navRecord, CopyRecordOptions.None);
				navRecord2.SetFieldValue(1, NavInteger.Default);
				navRecord2.SetFieldValue(2, aLRecordId);
				string text = (destinationRecord.MetaTable.DataPerCompany ? destinationRecord.CompanyName : string.Empty);
				NavText newValue = new NavText(text.Length, text);
				navRecord2.SetFieldValue(12, newValue);
				navRecord2.ALInsert(runApplicationTrigger: false);
			}
			while (navRecord.ALNext() != 0);
		}
	}

	public static bool HasLinks(NavRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (!IsRecordLinkTableLocked(record) && !TableHasLinks(record, record.MetaTable, record.CompanyName))
		{
			return false;
		}
		using NavRecord navRecord = new NavRecord(record, 2000000068, SecurityFiltering.Filtered);
		SetupRecordLinkRanges(navRecord, record);
		return !navRecord.ALIsEmpty;
	}

	public static void MoveLinks(NavRecord sourceRecord, NavRecord destinationRecord)
	{
		if (sourceRecord == null)
		{
			throw new ArgumentNullException("sourceRecord");
		}
		if (destinationRecord == null)
		{
			throw new ArgumentNullException("destinationRecord");
		}
		if (!IsRecordLinkTableLocked(sourceRecord) && !TableHasLinks(sourceRecord, sourceRecord.MetaTable, sourceRecord.CompanyName))
		{
			return;
		}
		using NavRecord navRecord = new NavRecord(sourceRecord, 2000000068);
		SetupRecordLinkRanges(navRecord, sourceRecord);
		NavText navText = (destinationRecord.MetaTable.DataPerCompany ? new NavText(destinationRecord.CompanyName.Length, destinationRecord.CompanyName) : null);
		NavRecordId aLRecordId = destinationRecord.ALRecordId;
		NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(2);
		NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(12);
		FieldDictionary<NavValue> newFieldValues = ((navText == null) ? new FieldDictionary<NavValue>(new Tuple<INavFieldMetadata, NavValue>(fieldByNo, aLRecordId)) : new FieldDictionary<NavValue>(new Tuple<INavFieldMetadata, NavValue>(fieldByNo, aLRecordId), new Tuple<INavFieldMetadata, NavValue>(fieldByNo2, navText)));
		navRecord.RecordImplementation.ModifyOrRenameAllRecords(newFieldValues);
	}

	public static void DeleteLinks(NavRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (!IsRecordLinkTableLocked(record) && !TableHasLinks(record, record.MetaTable, record.CompanyName))
		{
			return;
		}
		using NavRecord navRecord = new NavRecord(record, 2000000068);
		SetupRecordLinkRanges(navRecord, record);
		navRecord.ALLockTable();
		navRecord.ALDeleteAll(runTrigger: false);
	}

	private static void SetupRecordLinkRanges(NavRecord recordLink, NavRecord record)
	{
		NCLMetaTable metaTableById = record.Session.NCLMetadata.GetMetaTableById(2000000068);
		recordLink.SetRange(metaTableById.GetFieldByNo(2), record.ALRecordId, null);
		if (record.GetNCLMetaTable().DataPerCompany)
		{
			recordLink.SetRange(metaTableById.GetFieldByNo(12), new NavText(record.CompanyName.Length, record.CompanyName), null);
		}
	}

	public static bool TableHasLinks(ITreeObject parentTreeObject, NCLMetaTable targetTable, string companyName)
	{
		using NavRecord navRecord = new NavRecord(parentTreeObject, 2000000068, SecurityFiltering.Filtered);
		if (!IsRecordLinkTableLocked(parentTreeObject) && navRecord.ALIsEmpty)
		{
			return false;
		}
		NavRecordId navRecordId = NavRecordId.CreateFromValues(targetTable.TableId, new NavValue[0]);
		NavRecordId navRecordId2 = NavRecordId.CreateFromValues(targetTable.TableId + 1, new NavValue[0]);
		navRecord.SetFilter(navRecord.MetaTable.GetFieldByNo(2), ">=%1 & <%2", navRecordId, navRecordId2);
		if (targetTable.DataPerCompany)
		{
			navRecord.SetRange(navRecord.MetaTable.GetFieldByNo(12), new NavText(companyName.Length, companyName), null);
		}
		return !navRecord.ALIsEmpty;
	}

	private static bool IsRecordLinkTableLocked(ITreeObject parentTreeObject)
	{
		return parentTreeObject.Tree.Session.DataAccessSource.TenantDataAccess.IsTableLocked(2000000068, 0);
	}
}
