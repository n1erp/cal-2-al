using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.XmlMetadata;

internal static class DatabaseNavConfigurationRepository
{
	public static NavAppObjectMetadataRuntimeDeltas GetBaseConfigurationDelta(NavProfileKey profileKey, int pageId)
	{
		NavAppObjectMetadataRuntimeDeltas result = null;
		NavSession session = NavCurrentThread.Session;
		using (NavRecord navRecord = new NavRecord(session, 2000000186))
		{
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(1);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(2);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(3);
			navRecord.SetFilter(fieldByNo, "%1", new NavOemCode(2048, profileKey.ProfileId));
			navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create(pageId));
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				navRecord.ALCalcFields(fieldByNo3.FieldNo);
				NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(fieldByNo3);
				if (navBLOB.ALHasValue)
				{
					byte[] bytes = navBLOB.GetBytes();
					try
					{
						XDocument xDocument = XDocument.Parse(Encoding.UTF8.GetString(bytes));
						result = NavAppObjectMetadataRuntimeDeltas.FromXml(xDocument);
					}
					catch (InvalidOperationException ex)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000010Z", Category.Extensions, ex);
						throw new NavNSInvalidPageDefinitionException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.CannotDeserializeThePageDefinition, pageId), ex);
					}
				}
			}
		}
		return result;
	}

	public static NavAppObjectMetadataRuntimeDeltas GetTenantConfigurationDelta(NavProfileKey profileKey, int pageId, ProfilePageMetadataOwner Owner)
	{
		NavAppObjectMetadataRuntimeDeltas result = null;
		NavSession session = NavCurrentThread.Session;
		using (NavRecord navRecord = new NavRecord(session, 2000000187))
		{
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(2);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(3);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(4);
			NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(1);
			NCLMetaField fieldByNo5 = navRecord.MetaTable.GetFieldByNo(6);
			navRecord.SetFilter(fieldByNo, "%1", new NavOemCode(2048, profileKey.ProfileId));
			navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create(pageId));
			navRecord.SetFilter(fieldByNo4, "%1", new NavGuid(profileKey.AppId.Value));
			navRecord.SetFilter(fieldByNo5, "%1", NavOption.Create(fieldByNo5.FieldOptionMetadata, (int)Owner));
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				navRecord.ALCalcFields(fieldByNo3.FieldNo);
				NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(fieldByNo3);
				if (navBLOB.ALHasValue)
				{
					byte[] bytes = navBLOB.GetBytes();
					try
					{
						XDocument xDocument = XDocument.Parse(Encoding.UTF8.GetString(bytes));
						result = NavAppObjectMetadataRuntimeDeltas.FromXml(xDocument);
					}
					catch (InvalidOperationException ex)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000110", Category.Extensions, ex);
						throw new NavNSInvalidPageDefinitionException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.CannotDeserializeThePageDefinition, pageId), ex);
					}
				}
			}
		}
		return result;
	}

	public static IEnumerable<string> GetBaseConfigurationALCode(NavProfileKey profileKey, int? pageId = null)
	{
		List<string> list = new List<string>();
		NavSession session = NavCurrentThread.Session;
		using (NavRecord navRecord = new NavRecord(session, 2000000186))
		{
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(1);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(2);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(4);
			navRecord.SetFilter(fieldByNo, "%1", new NavOemCode(2048, profileKey.ProfileId));
			if (pageId.HasValue)
			{
				navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create(pageId.Value));
			}
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				do
				{
					navRecord.ALCalcFields(fieldByNo3.FieldNo);
					NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(fieldByNo3);
					if (navBLOB.ALHasValue)
					{
						byte[] bytes = navBLOB.GetBytes();
						list.Add(Encoding.UTF8.GetString(bytes));
					}
				}
				while (navRecord.NextRecord());
			}
		}
		return list;
	}

	public static IEnumerable<string> GetTenantConfigurationALCode(NavProfileKey profileKey, ProfilePageMetadataOwner owner, int? pageId = null)
	{
		List<string> list = new List<string>();
		NavSession session = NavCurrentThread.Session;
		using (NavRecord navRecord = new NavRecord(session, 2000000187))
		{
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(2);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(3);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(5);
			NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(1);
			NCLMetaField fieldByNo5 = navRecord.MetaTable.GetFieldByNo(6);
			navRecord.SetFilter(fieldByNo, "%1", new NavOemCode(2048, profileKey.ProfileId));
			navRecord.SetFilter(fieldByNo4, "%1", new NavGuid(profileKey.AppId.Value));
			navRecord.SetFilter(fieldByNo5, "%1", NavOption.Create(fieldByNo5.FieldOptionMetadata, (int)owner));
			if (pageId.HasValue)
			{
				navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create(pageId.Value));
			}
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				do
				{
					navRecord.ALCalcFields(fieldByNo3.FieldNo);
					NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(fieldByNo3);
					if (navBLOB.ALHasValue)
					{
						byte[] bytes = navBLOB.GetBytes();
						list.Add(Encoding.UTF8.GetString(bytes));
					}
				}
				while (navRecord.NextRecord());
			}
		}
		return list;
	}

	public static void SaveBaseConfiguration(NavProfileKey profileKey, int pageId, string delta, string alCode)
	{
		NavSession session = NavCurrentThread.Session;
		using NavRecord navRecord = new NavRecord(session, 2000000186);
		NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(1);
		NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(2);
		NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(3);
		NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(4);
		navRecord.SetFilter(fieldByNo, "%1", new NavOemCode(2048, profileKey.ProfileId));
		navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create(pageId));
		session.BeginTransaction();
		bool flag = navRecord.ALFindFirst(DataError.TrapError);
		NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(fieldByNo3);
		navBLOB.AssignFromBytes(Encoding.UTF8.GetBytes(delta));
		NavBLOB navBLOB2 = (NavBLOB)navRecord.GetFieldValue(fieldByNo4);
		navBLOB2.AssignFromBytes(Encoding.UTF8.GetBytes(alCode));
		if (flag)
		{
			navRecord.ALModify(DataError.ThrowError, runApplicationTrigger: true);
		}
		else
		{
			navRecord.SetFieldValue(fieldByNo, new NavOemCode(2048, profileKey.ProfileId ?? NavConfigurationProfile.GetUserProfile(NavCurrentThread.Session.User.Id).ProfileId));
			navRecord.SetFieldValue(fieldByNo2, NavInteger.Create(pageId));
			navRecord.ALInsert(runApplicationTrigger: true);
		}
		session.EndTransaction(commit: true);
	}

	public static void SaveTenantConfiguration(NavProfileKey profileKey, int pageId, string delta, string alCode, ProfilePageMetadataOwner owner)
	{
		NavSession session = NavCurrentThread.Session;
		using NavRecord navRecord = new NavRecord(session, 2000000187);
		NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(2);
		NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(3);
		NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(4);
		NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(5);
		NCLMetaField fieldByNo5 = navRecord.MetaTable.GetFieldByNo(1);
		NCLMetaField fieldByNo6 = navRecord.MetaTable.GetFieldByNo(6);
		navRecord.SetFilter(fieldByNo, "%1", new NavOemCode(2048, profileKey.ProfileId));
		navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create(pageId));
		navRecord.SetFilter(fieldByNo5, "%1", profileKey.AppId);
		navRecord.SetFilter(fieldByNo6, "%1", NavOption.Create(fieldByNo6.FieldOptionMetadata, (int)owner));
		session.BeginTransaction();
		bool flag = navRecord.ALFindFirst(DataError.TrapError);
		NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(fieldByNo3);
		navBLOB.AssignFromBytes(Encoding.UTF8.GetBytes(delta));
		NavBLOB navBLOB2 = (NavBLOB)navRecord.GetFieldValue(fieldByNo4);
		navBLOB2.AssignFromBytes(Encoding.UTF8.GetBytes(alCode));
		if (flag)
		{
			navRecord.ALModify(DataError.ThrowError, runApplicationTrigger: true);
		}
		else
		{
			navRecord.SetFieldValue(fieldByNo, new NavOemCode(2048, profileKey.ProfileId ?? NavConfigurationProfile.GetUserProfile(NavCurrentThread.Session.User.Id).ProfileId));
			navRecord.SetFieldValue(fieldByNo2, NavInteger.Create(pageId));
			navRecord.SetFieldValue(fieldByNo5, profileKey.AppId);
			navRecord.SetFieldValue(fieldByNo6, NavOption.Create(fieldByNo6.FieldOptionMetadata, (int)owner));
			navRecord.ALInsert(runApplicationTrigger: true);
		}
		session.EndTransaction(commit: true);
	}

	public static void DeleteConfiguration(NavProfileKey profileKey, int pageId, ProfilePageMetadataOwner owner)
	{
		NavSession session = NavCurrentThread.Session;
		int tableId;
		int fieldNo;
		int fieldNo2;
		if (owner == ProfilePageMetadataOwner.SystemOrApp)
		{
			tableId = 2000000186;
			fieldNo = 1;
			fieldNo2 = 2;
		}
		else
		{
			tableId = 2000000187;
			fieldNo = 2;
			fieldNo2 = 3;
		}
		using NavRecord navRecord = new NavRecord(session, tableId);
		NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(fieldNo);
		NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(fieldNo2);
		navRecord.SetFilter(fieldByNo, "%1", new NavOemCode(2048, profileKey.ProfileId));
		navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create(pageId));
		if (navRecord.ALFindFirst(DataError.TrapError))
		{
			session.BeginTransaction();
			navRecord.ALDelete(DataError.ThrowError, runApplicationTrigger: true);
			session.EndTransaction(commit: true);
		}
	}
}
