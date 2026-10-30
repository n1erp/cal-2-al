using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

internal class DatabaseNavV1PageConfigurationRepository : INavV1PageConfiguration
{
	private static readonly XmlSerializer serializer = new XmlSerializer(typeof(NavXmlDelta));

	public NavXmlDelta LoadPersonalizationDelta(int pageId, string personalizationId)
	{
		return LoadPersonalizationDelta(NavCurrentThread.Session.User.Id, pageId, personalizationId);
	}

	public NavXmlDelta LoadPersonalizationDelta(Guid userid, int pageId, string personalizationId)
	{
		personalizationId = AdjustPersonalizationId(personalizationId, pageId);
		return LoadPersonalizationDeltas((NavRecord record) =>
		{
			NCLMetaField fieldByNo = record.MetaTable.GetFieldByNo(3);
			NCLMetaField fieldByNo2 = record.MetaTable.GetFieldByNo(9);
			NCLMetaField fieldByNo3 = record.MetaTable.GetFieldByNo(21);
			record.SetFilter(fieldByNo, "%1", new NavGuid(userid));
			record.SetFilter(fieldByNo2, "%1", NavInteger.Create(pageId));
			record.SetFilter(fieldByNo3, "%1", new NavOemCode(2048, personalizationId));
		}).FirstOrDefault()?.PageMetadataDelta;
	}

	public IEnumerable<UserMetadataRecord> LoadPersonalizationDeltasForUser(Guid userId, string personalizationId)
	{
		return LoadPersonalizationDeltas((NavRecord record) =>
		{
			if (!string.IsNullOrEmpty(personalizationId))
			{
				NCLMetaField fieldByNo = record.MetaTable.GetFieldByNo(21);
				record.SetFilter(fieldByNo, "%1", new NavOemCode(2048, personalizationId));
			}
			NCLMetaField fieldByNo2 = record.MetaTable.GetFieldByNo(3);
			record.SetFilter(fieldByNo2, "%1", new NavGuid(userId));
		});
	}

	private static IList<UserMetadataRecord> LoadPersonalizationDeltas(Action<NavRecord> setfilterAction)
	{
		List<UserMetadataRecord> list = new List<UserMetadataRecord>();
		using (NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000075))
		{
			setfilterAction?.Invoke(navRecord);
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(9);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(21);
			NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(24);
			navRecord.SetAutoCalcFields(DataError.ThrowError, new int[1] { fieldByNo4.FieldNo }, replace: true);
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				do
				{
					int value = ((NavInteger)navRecord.GetFieldValue(fieldByNo2)).Value;
					NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(fieldByNo4);
					if (navBLOB.GetBytesSize <= 0)
					{
						continue;
					}
					byte[] bytes = navBLOB.GetBytes();
					NavXmlDelta pageMetadataDelta;
					using (MemoryStream documentStream = new MemoryStream(bytes))
					{
						try
						{
							pageMetadataDelta = (NavXmlDelta)serializer.Deserialize(XmlHelper.SafeXmlReader(documentStream));
						}
						catch (InvalidOperationException ex)
						{
							LogWriterHelper.LogExceptionEvent(ex);
							throw new NavNSInvalidPageDefinitionException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.CannotDeserializeThePageDefinition, value), ex);
						}
					}
					list.Add(new UserMetadataRecord
					{
						UserSid = ((NavGuid)navRecord.GetFieldValue(fieldByNo)).Value,
						PageId = value,
						PersonalizationId = ((NavCode)navRecord.GetFieldValue(fieldByNo3)).Value,
						PageMetadataDelta = pageMetadataDelta
					});
				}
				while (navRecord.ALNext() > 0);
			}
		}
		return list;
	}

	public void SavePersonalizationDelta(int pageId, string personalizationId, NavXmlDelta delta)
	{
		SavePersonalizationDelta(NavCurrentThread.Session.User, pageId, personalizationId, delta);
	}

	public void SavePersonalizationDelta(NavUser user, int pageId, string personalizationId, NavXmlDelta delta)
	{
		SavePersonalizationDelta(user.Id, pageId, personalizationId, delta);
	}

	public void SavePersonalizationDelta(Guid userId, int pageId, string personalizationId, NavXmlDelta delta)
	{
		NavSession session = NavCurrentThread.Session;
		using NavRecord navRecord = new NavRecord(session, 2000000075);
		NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(9);
		NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(21);
		NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(24);
		personalizationId = AdjustPersonalizationId(personalizationId, pageId);
		navRecord.SetFilter(fieldByNo, "%1", new NavGuid(userId));
		navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create(pageId));
		navRecord.SetFilter(fieldByNo3, "%1", new NavOemCode(2048, personalizationId));
		session.BeginTransaction();
		bool flag = navRecord.ALFindFirst(DataError.TrapError);
		NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(fieldByNo4);
		using (NavOutStream navOutStream = navBLOB.ALCreateOutStream(navRecord))
		{
			serializer.Serialize(navOutStream.InternalStreamCheckedForWrite, delta);
		}
		if (flag)
		{
			navRecord.ALModify(DataError.ThrowError, runApplicationTrigger: true);
		}
		else
		{
			navRecord.SetFieldValue(fieldByNo, new NavGuid(userId));
			navRecord.SetFieldValue(fieldByNo2, NavInteger.Create(pageId));
			navRecord.SetFieldValue(fieldByNo3, new NavOemCode(2048, personalizationId));
			navRecord.ALInsert(runApplicationTrigger: true);
		}
		session.EndTransaction(commit: true);
		NavCurrentThread.Session.UserPageMetadataCacheV1.Remove(pageId, personalizationId);
	}

	public IEnumerable<ProfileMetadataRecord> LoadConfigurationDeltasForProfile(NavProfileKey profileKey, string configurationId)
	{
		if (profileKey == null || string.IsNullOrEmpty(profileKey.ProfileId) || profileKey.Scope == ProfileScope.Tenant)
		{
			return null;
		}
		return LoadConfigurationDelta((NavRecord record) =>
		{
			if (!string.IsNullOrEmpty(configurationId))
			{
				NCLMetaField fieldByNo = record.MetaTable.GetFieldByNo(18);
				record.SetFilter(fieldByNo, "%1", new NavOemCode(2048, configurationId));
			}
			NCLMetaField fieldByNo2 = record.MetaTable.GetFieldByNo(3);
			record.SetFilter(fieldByNo2, "%1", new NavOemCode(2048, profileKey.ProfileId));
		});
	}

	private static IList<ProfileMetadataRecord> LoadConfigurationDelta(Action<NavRecord> setfilterAction)
	{
		List<ProfileMetadataRecord> list = new List<ProfileMetadataRecord>();
		using (NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000074))
		{
			setfilterAction?.Invoke(navRecord);
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(6);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(18);
			NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(21);
			navRecord.SetAutoCalcFields(DataError.ThrowError, new int[1] { fieldByNo4.FieldNo }, replace: true);
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				do
				{
					int value = ((NavInteger)navRecord.GetFieldValue(fieldByNo2)).Value;
					NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(fieldByNo4);
					if (navBLOB.GetBytesSize <= 0)
					{
						continue;
					}
					byte[] bytes = navBLOB.GetBytes();
					NavXmlDelta pageMetadataDelta;
					using (MemoryStream documentStream = new MemoryStream(bytes))
					{
						try
						{
							pageMetadataDelta = (NavXmlDelta)serializer.Deserialize(XmlHelper.SafeXmlReader(documentStream));
						}
						catch (InvalidOperationException ex)
						{
							LogWriterHelper.LogExceptionEvent(ex);
							throw new NavNSInvalidPageDefinitionException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.CannotDeserializeThePageDefinition, value), ex);
						}
					}
					list.Add(new ProfileMetadataRecord
					{
						ProfileId = ((NavCode)navRecord.GetFieldValue(fieldByNo)).Value,
						PageId = value,
						PersonalizationId = ((NavCode)navRecord.GetFieldValue(fieldByNo3)).Value,
						PageMetadataDelta = pageMetadataDelta
					});
				}
				while (navRecord.ALNext() > 0);
			}
		}
		return list;
	}

	public NavXmlDelta LoadConfigurationDelta(NavProfileKey profileKey, int pageId, string personalizationId)
	{
		if (profileKey == null || string.IsNullOrEmpty(profileKey.ProfileId) || profileKey.Scope == ProfileScope.Tenant)
		{
			return null;
		}
		NavXmlDelta navXmlDelta = null;
		NavSession session = NavCurrentThread.Session;
		int num = 5;
		for (int i = 1; i <= num; i++)
		{
			using (NavRecord navRecord = new NavRecord(session, 2000000074))
			{
				NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
				NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(6);
				NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(18);
				NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(21);
				personalizationId = AdjustPersonalizationId(personalizationId, pageId);
				navRecord.SetFilter(fieldByNo, "%1", new NavOemCode(2048, profileKey.ProfileId));
				navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create(pageId));
				navRecord.SetFilter(fieldByNo3, "%1", new NavOemCode(2048, personalizationId));
				if (navRecord.ALFindFirst(DataError.TrapError))
				{
					navRecord.ALCalcFields(fieldByNo4.FieldNo);
					NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(fieldByNo4);
					if (navBLOB.ALHasValue)
					{
						byte[] bytes = navBLOB.GetBytes();
						using MemoryStream documentStream = new MemoryStream(bytes);
						try
						{
							navXmlDelta = (NavXmlDelta)serializer.Deserialize(XmlHelper.SafeXmlReader(documentStream));
						}
						catch (InvalidOperationException ex)
						{
							LogWriterHelper.LogExceptionEvent(ex);
							if (i == num)
							{
								throw new NavNSInvalidPageDefinitionException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.CannotDeserializeThePageDefinition, pageId), ex);
							}
						}
					}
				}
			}
			if (navXmlDelta != null)
			{
				break;
			}
		}
		return navXmlDelta;
	}

	public void SaveConfigurationDelta(NavProfileKey profileKey, int pageId, string personalizationId, NavXmlDelta delta)
	{
		if (profileKey.Scope == ProfileScope.Tenant)
		{
			throw new NavProfileException(Lang.ConfigurationChangesOnTenantProfileNotAllowed);
		}
		NavSession session = NavCurrentThread.Session;
		using NavRecord navRecord = new NavRecord(session, 2000000074);
		NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(6);
		NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(18);
		NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(21);
		personalizationId = AdjustPersonalizationId(personalizationId, pageId);
		navRecord.SetFilter(fieldByNo, "%1", new NavOemCode(2048, profileKey.ProfileId));
		navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create(pageId));
		navRecord.SetFilter(fieldByNo3, "%1", new NavOemCode(2048, personalizationId));
		session.BeginTransaction();
		bool flag = navRecord.ALFindFirst(DataError.TrapError);
		NavBLOB navBLOB = (NavBLOB)navRecord.GetFieldValue(fieldByNo4);
		using (NavOutStream navOutStream = navBLOB.ALCreateOutStream(navRecord))
		{
			serializer.Serialize(navOutStream.InternalStreamCheckedForWrite, delta);
		}
		if (flag)
		{
			navRecord.ALModify(DataError.ThrowError, runApplicationTrigger: true);
		}
		else
		{
			navRecord.SetFieldValue(fieldByNo, new NavOemCode(2048, profileKey.ProfileId));
			navRecord.SetFieldValue(fieldByNo2, NavInteger.Create(pageId));
			navRecord.SetFieldValue(fieldByNo3, new NavOemCode(2048, personalizationId));
			navRecord.ALInsert(runApplicationTrigger: true);
		}
		session.EndTransaction(commit: true);
	}

	public void DeleteConfiguration(NavProfileKey profileKey, int pageId, string personalizationId)
	{
		if (profileKey.Scope == ProfileScope.Tenant)
		{
			throw new NavProfileException(Lang.ConfigurationChangesOnTenantProfileNotAllowed);
		}
		NavSession session = NavCurrentThread.Session;
		using NavRecord navRecord = new NavRecord(session, 2000000074);
		NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(6);
		NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(18);
		personalizationId = AdjustPersonalizationId(personalizationId, pageId);
		navRecord.SetFilter(fieldByNo, "%1", new NavOemCode(2048, profileKey.ProfileId));
		navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create(pageId));
		navRecord.SetFilter(fieldByNo3, "%1", new NavOemCode(2048, personalizationId));
		if (navRecord.ALFindFirst(DataError.TrapError))
		{
			session.BeginTransaction();
			navRecord.ALDelete(DataError.ThrowError, runApplicationTrigger: true);
			session.EndTransaction(commit: true);
		}
	}

	public void DeletePersonalization(int pageId, string personalizationId)
	{
		DeletePersonalization(NavCurrentThread.Session.User.Id, pageId, personalizationId);
	}

	public void DeletePersonalization(Guid userId, int pageId, string personalizationId)
	{
		NavSession session = NavCurrentThread.Session;
		using NavRecord navRecord = new NavRecord(session, 2000000075);
		NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(9);
		NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(21);
		personalizationId = AdjustPersonalizationId(personalizationId, pageId);
		navRecord.SetFilter(fieldByNo, "%1", new NavGuid(userId));
		navRecord.SetFilter(fieldByNo2, "%1", NavInteger.Create(pageId));
		navRecord.SetFilter(fieldByNo3, "%1", new NavOemCode(2048, personalizationId));
		if (navRecord.ALFindFirst(DataError.TrapError))
		{
			session.BeginTransaction();
			navRecord.ALDelete(DataError.ThrowError, runApplicationTrigger: true);
			session.EndTransaction(commit: true);
		}
	}

	internal static string AdjustPersonalizationId(string personalizationId, int pageId)
	{
		if (!string.IsNullOrEmpty(personalizationId))
		{
			return personalizationId;
		}
		return pageId.ToString(CultureInfo.InvariantCulture);
	}
}
