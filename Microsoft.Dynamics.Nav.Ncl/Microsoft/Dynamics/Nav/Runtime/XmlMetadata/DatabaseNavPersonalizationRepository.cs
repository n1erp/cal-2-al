using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.XmlMetadata;

internal static class DatabaseNavPersonalizationRepository
{
	public static NavAppObjectMetadataRuntimeDeltas GetPersonalizationDelta(Guid userId, int pageId)
	{
		NavAppObjectMetadataRuntimeDeltas result = null;
		NavSession session = NavCurrentThread.Session;
		using (new MaximizedPermissionScope(session, new int[1] { 2000000188 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000188);
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(1);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(2);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(3);
			navRecord.SetFilter(fieldByNo, "%1", new NavGuid(userId));
			navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create(pageId));
			navRecord.SetAutoCalcFields(DataError.ThrowError, new int[1] { fieldByNo3.FieldNo }, replace: true);
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				NavBLOB navBLOB = new NavBLOB(((NavBLOB)navRecord.GetFieldValue(fieldByNo3))?.GetBytes() ?? new byte[0]);
				int getBytesSize = navBLOB.GetBytesSize;
				if (getBytesSize > 0)
				{
					byte[] bytes = navBLOB.GetBytes();
					try
					{
						XDocument xDocument = XDocument.Parse(Encoding.UTF8.GetString(bytes));
						result = NavAppObjectMetadataRuntimeDeltas.FromXml(xDocument);
					}
					catch (InvalidOperationException ex)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000010Y", Category.Extensions, ex);
						throw new NavNSInvalidPageDefinitionException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.CannotDeserializeThePageDefinition, pageId), ex);
					}
				}
			}
		}
		return result;
	}

	public static IEnumerable<string> GetPersonalizationALCode(Guid userId)
	{
		List<string> list = new List<string>();
		NavSession session = NavCurrentThread.Session;
		using (new MaximizedPermissionScope(session, new int[1] { 2000000188 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000188);
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(1);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(4);
			navRecord.SetFilter(fieldByNo, "%1", new NavGuid(userId));
			navRecord.SetAutoCalcFields(DataError.ThrowError, new int[1] { fieldByNo2.FieldNo }, replace: true);
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				do
				{
					NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(fieldByNo2);
					int getBytesSize = navBLOB.GetBytesSize;
					if (getBytesSize > 0)
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

	public static void SavePersonalization(Guid userId, int pageId, string delta, string alCode)
	{
		NavSession session = NavCurrentThread.Session;
		using (new MaximizedPermissionScope(session, new int[1] { 2000000188 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000188);
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(1);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(2);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(3);
			NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(4);
			navRecord.SetFilter(fieldByNo, "%1", new NavGuid(userId));
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
				navRecord.SetFieldValue(fieldByNo, new NavGuid(userId));
				navRecord.SetFieldValue(fieldByNo2, NavInteger.Create(pageId));
				navRecord.ALInsert(runApplicationTrigger: true);
			}
			session.EndTransaction(commit: true);
		}
	}

	public static void DeletePersonalization(Guid userId, int pageId)
	{
		NavSession session = NavCurrentThread.Session;
		using (new MaximizedPermissionScope(session, new int[1] { 2000000188 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000188);
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(1);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(2);
			navRecord.SetFilter(fieldByNo, "%1", new NavGuid(userId));
			navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create(pageId));
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				session.BeginTransaction();
				navRecord.ALDelete(DataError.ThrowError, runApplicationTrigger: true);
				session.EndTransaction(commit: true);
			}
		}
	}
}
