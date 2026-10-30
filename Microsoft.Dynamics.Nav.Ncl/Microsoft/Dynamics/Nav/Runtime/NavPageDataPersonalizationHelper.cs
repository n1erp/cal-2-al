using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavPageDataPersonalizationHelper
{
	public static Dictionary<string, NavValue> ReadValues(int objectId, PageType pageType, PageDataIdentifier pageDataIdentifier)
	{
		ObjectType objectType = MapPageTypeToObjectType(pageType);
		return LoadPageDataPersonalization(NavCurrentThread.FormPersonalizationId, objectId, objectType, pageDataIdentifier, SaveValuesSerializer.Deserialize);
	}

	public static string ReadValue(int objectId, ObjectType objectType, string personalizationId, PageDataIdentifier pageDataIdentifier)
	{
		return LoadPageDataPersonalization(personalizationId, objectId, objectType, pageDataIdentifier, (byte[] bytes) => Encoding.UTF8.GetString(bytes));
	}

	public static void SaveValue(string personalizationId, int objectId, ObjectType objectType, string pageDataIdentifier, string objectValue)
	{
		try
		{
			SavePageDataPersonalization(objectId, objectType, personalizationId, (PageDataIdentifier)Enum.Parse(typeof(PageDataIdentifier), pageDataIdentifier), () => Encoding.UTF8.GetBytes(objectValue));
		}
		catch (Exception exception)
		{
			string content = string.Format(CultureInfo.InvariantCulture, "SaveValue failed for object type {0} with id {1}.", objectType, objectId);
			NavCurrentThread.Session.Diagnostics.SendExceptionTag("0000208", Category.Runtime, exception, Verbosity.Warning, DiagnosticParameter.SystemMetadata(content));
		}
	}

	public static void SaveValues(int objectId, PageType pageType, PageDataIdentifier pageDataIdentifier, Dictionary<string, NavValue> values)
	{
		try
		{
			string formPersonalizationId = NavCurrentThread.FormPersonalizationId;
			ObjectType objectType = MapPageTypeToObjectType(pageType);
			SavePageDataPersonalization(objectId, objectType, formPersonalizationId, pageDataIdentifier, () => SaveValuesSerializer.Serialize(values));
		}
		catch (Exception exception)
		{
			string content = string.Format(CultureInfo.InvariantCulture, "SaveValue failed for page {0}.", objectId);
			NavCurrentThread.Session.Diagnostics.SendExceptionTag("0000209", Category.Runtime, exception, Verbosity.Warning, DiagnosticParameter.SystemMetadata(content));
		}
	}

	private static ObjectType MapPageTypeToObjectType(PageType pageType)
	{
		return pageType switch
		{
			PageType.ReportPreview => ObjectType.Report, 
			PageType.XmlPort => ObjectType.XmlPort, 
			_ => ObjectType.Page, 
		};
	}

	private static T LoadPageDataPersonalization<T>(string personalizationId, int objectId, ObjectType objectType, PageDataIdentifier pageDataIdentifier, Func<byte[], T> deserialize)
	{
		NavSession session = NavCurrentThread.Session;
		T result = default;
		if (session != null)
		{
			using NavRecord navRecord = new NavRecord(session, 2000000080);
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(9);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(12);
			NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(21);
			NCLMetaField fieldByNo5 = navRecord.MetaTable.GetFieldByNo(24);
			NCLMetaField fieldByNo6 = navRecord.MetaTable.GetFieldByNo(27);
			if (string.IsNullOrEmpty(personalizationId))
			{
				personalizationId = objectId.ToString(CultureInfo.InvariantCulture);
			}
			navRecord.SetFilter(fieldByNo, "%1", new NavGuid(NavCurrentThread.Session.User.Id));
			navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create((int)objectType));
			navRecord.SetFilter(fieldByNo3, "%1", NavInteger.Create(objectId));
			navRecord.SetFilter(fieldByNo4, "%1", new NavCode(2048, personalizationId));
			navRecord.SetFilter(fieldByNo5, "%1", new NavCode(2048, pageDataIdentifier.ToString()));
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				navRecord.ALCalcFields(fieldByNo6.FieldNo);
				NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(fieldByNo6);
				int getBytesSize = navBLOB.GetBytesSize;
				if (getBytesSize > 0)
				{
					try
					{
						result = deserialize(navBLOB.GetBytes());
						return result;
					}
					catch (Exception exception)
					{
						string content = string.Format(CultureInfo.InvariantCulture, "Deserializing page data personalization failed for object type {0} with id {1}.", objectType, objectId);
						DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000020A", Category.Runtime, exception, Verbosity.Warning, DiagnosticParameter.SystemMetadata(content));
					}
				}
			}
		}
		return result;
	}

	private static void SavePageDataPersonalization(int objectId, ObjectType objectType, string personalizationId, PageDataIdentifier pageDataIdentifier, Func<byte[]> serialize)
	{
		if (objectId < 0 || serialize == null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000020C", Category.Runtime, Verbosity.Normal, "Invalid object id or serializer delegate, id = {0}", DiagnosticParameter.SystemMetadata(objectId));
			return;
		}
		byte[] bytes;
		try
		{
			bytes = serialize();
		}
		catch (Exception exception)
		{
			string content = string.Format(CultureInfo.InvariantCulture, "Saving page data personalization failed for object type {0} with id {1}.", objectType, objectId);
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000020B", Category.Runtime, exception, Verbosity.Warning, DiagnosticParameter.SystemMetadata(content));
			return;
		}
		NavSession session = NavCurrentThread.Session;
		using NavRecord navRecord = new NavRecord(session, 2000000080);
		NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(9);
		NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(12);
		NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(21);
		NCLMetaField fieldByNo5 = navRecord.MetaTable.GetFieldByNo(6);
		NCLMetaField fieldByNo6 = navRecord.MetaTable.GetFieldByNo(24);
		NCLMetaField fieldByNo7 = navRecord.MetaTable.GetFieldByNo(27);
		if (string.IsNullOrEmpty(personalizationId))
		{
			personalizationId = objectId.ToString(CultureInfo.InvariantCulture);
		}
		navRecord.SetFilter(fieldByNo, "%1", new NavGuid(NavCurrentThread.Session.User.Id));
		navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create((int)objectType));
		navRecord.SetFilter(fieldByNo3, "%1", NavInteger.Create(objectId));
		navRecord.SetFilter(fieldByNo4, "%1", new NavCode(2048, personalizationId));
		navRecord.SetFilter(fieldByNo6, "%1", new NavCode(2048, pageDataIdentifier.ToString()));
		bool commit = false;
		session.BeginTransaction();
		try
		{
			bool flag = navRecord.ALFindFirst(DataError.TrapError);
			NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(fieldByNo7);
			navBLOB.AssignFromBytes(bytes);
			if (flag)
			{
				try
				{
					navRecord.ALModify(DataError.ThrowError);
				}
				catch (NavCSideException ex) when (ex.ErrorCode == 85132286)
				{
				}
			}
			else
			{
				string name = NavCurrentThread.Session.User.Name;
				navRecord.SetFieldValue(fieldByNo, new NavGuid(NavCurrentThread.Session.User.Id));
				navRecord.SetFieldValue(fieldByNo2, NavInteger.Create((int)objectType));
				navRecord.SetFieldValue(fieldByNo3, NavInteger.Create(objectId));
				navRecord.SetFieldValue(fieldByNo4, new NavCode(2048, personalizationId));
				navRecord.SetFieldValue(fieldByNo5, new NavText(int.MaxValue, name));
				navRecord.SetFieldValue(fieldByNo6, new NavCode(2048, pageDataIdentifier.ToString()));
				navRecord.ALInsert(DataError.TrapError);
			}
			commit = true;
		}
		finally
		{
			session.EndTransaction(commit);
		}
	}
}
