using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal class NavMediaPurger
{
	private struct MediaMap
	{
		public NCLMetaField Field;

		public Guid MediaId;
	}

	private IList<MediaMap> mediaScheduledForRemoval;

	private IList<MediaMap> mediaSetScheduledForRemoval;

	private readonly NCLMetaTable metaTable;

	private readonly RecordImplementation recordImplementation;

	public NavMediaPurger(RecordImplementation recordImplementation, NCLMetaTable nclMetaTable)
	{
		this.recordImplementation = recordImplementation;
		metaTable = nclMetaTable;
	}

	internal void PurgeMediaLinks()
	{
		if (mediaScheduledForRemoval != null)
		{
			if (mediaScheduledForRemoval.Count > 0)
			{
				IList<Guid> mediaIdList = FindOrphanedMediaObjects(mediaScheduledForRemoval);
				DeleteMediaObjects(recordImplementation, mediaIdList, emitCallStackInLog: true);
			}
			mediaScheduledForRemoval.Clear();
			mediaScheduledForRemoval = null;
		}
		if (mediaSetScheduledForRemoval != null)
		{
			if (mediaSetScheduledForRemoval.Count > 0)
			{
				IList<Guid> mediaSets = FindOrphanedMediaObjects(mediaSetScheduledForRemoval);
				DeleteMediaSetObjects(recordImplementation, mediaSets, deleteContainedMedia: true);
			}
			mediaSetScheduledForRemoval.Clear();
			mediaSetScheduledForRemoval = null;
		}
	}

	private IList<Guid> FindOrphanedMediaObjects(ICollection<MediaMap> mediaList)
	{
		if (mediaList.Count == 0)
		{
			return new List<Guid>();
		}
		List<Guid> list = new List<Guid>(mediaList.Count);
		using NavRecord navRecord = metaTable.CreateObjectInstance(recordImplementation);
		foreach (MediaMap media in mediaList)
		{
			NCLMetaField field = media.Field;
			Guid mediaId = media.MediaId;
			navRecord.SetFilter(field, mediaId.ToString());
			if (!navRecord.ALFind(DataError.TrapError, "-"))
			{
				list.Add(media.MediaId);
			}
		}
		return list;
	}

	internal void MediaPurgeOnModifyList(MutableRecordBuffer mutableRecordBuffer)
	{
		NCLMetaTable nCLMetaTable = metaTable;
		if (nCLMetaTable == null || nCLMetaTable.MediaFieldCount <= 0 || metaTable.IsSystemOrVirtualTable)
		{
			return;
		}
		ReadOnlyRecordBuffer buffer = mutableRecordBuffer.ReadOnlyBuffer;
		IEnumerable<NCLMetaField> enumerable = from mediaField in metaTable.MediaFields
			where buffer[mediaField.FieldIndex] != null
			where !buffer[mediaField.FieldIndex].Equals(mutableRecordBuffer[mediaField.FieldIndex])
			select mediaField;
		foreach (NCLMetaField item in enumerable)
		{
			UpdateMediaDeleteList(item, buffer[item.FieldIndex].ToGuid());
		}
	}

	internal void DiscoverMediaLinks()
	{
		if (metaTable != null && metaTable.MediaFieldCount != 0 && !metaTable.IsMediaTable)
		{
			NCLMetaField[] mediaFields = metaTable.MediaFields;
			foreach (NCLMetaField nCLMetaField in mediaFields)
			{
				Guid mediaId = recordImplementation.GetFieldValue(nCLMetaField).ToGuid();
				UpdateMediaDeleteList(nCLMetaField, mediaId);
			}
		}
	}

	private void UpdateMediaDeleteList(NCLMetaField nclMetaField, Guid mediaId)
	{
		switch (nclMetaField.FieldNavType)
		{
		case NavType.Media:
			mediaScheduledForRemoval = BuildMediaDeleteListElement(mediaScheduledForRemoval, nclMetaField, mediaId);
			break;
		case NavType.MediaSet:
			mediaSetScheduledForRemoval = BuildMediaDeleteListElement(mediaSetScheduledForRemoval, nclMetaField, mediaId);
			break;
		}
	}

	private static IList<MediaMap> BuildMediaDeleteListElement(IList<MediaMap> mediaList, NCLMetaField nclMetaField, Guid mediaId)
	{
		if (mediaId.Equals(Guid.Empty))
		{
			return mediaList;
		}
		if (mediaList == null)
		{
			mediaList = new List<MediaMap>();
		}
		mediaList.Add(new MediaMap
		{
			Field = nclMetaField,
			MediaId = mediaId
		});
		return mediaList;
	}

	internal static int DeleteMediaSetObjects(ITreeObject parent, IList<Guid> mediaSets, bool deleteContainedMedia)
	{
		int num = 0;
		if (parent == null || mediaSets == null || mediaSets.Count == 0)
		{
			return 0;
		}
		RecordImplementation recordImplementation = parent as RecordImplementation;
		int mediaSetTableId = NavMediaHelper.GetMediaSetTableId(recordImplementation?.InstanceIdentifier.TableId ?? 0);
		LogDeletedObjectsInDiagnostics(recordImplementation, mediaSets, isSet: true, emitCallStackInLog: true);
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { mediaSetTableId }))
		{
			using NavRecordHandle navRecordHandle = new NavRecordHandle(parent, mediaSetTableId, temporary: false, SecurityFiltering.Validated);
			SetCompanyFilterOnMedia(recordImplementation, navRecordHandle.Target);
			List<Guid> list = null;
			foreach (NavGuid item in from mediaSetId in mediaSets.Where((Guid mediaSetId) =>
				{
					Guid guid = mediaSetId;
					return !guid.Equals(Guid.Empty);
				})
				select new NavGuid(mediaSetId))
			{
				navRecordHandle.Target.ALSetFilter(1, item.ToString());
				if (!navRecordHandle.Target.ALFind(DataError.TrapError, "-"))
				{
					continue;
				}
				bool flag = true;
				if (recordImplementation != null && !recordImplementation.DataPerCompany)
				{
					flag = NavMediaHelper.IsCompanyValid(navRecordHandle.Target, isMediaSet: true);
				}
				if (!flag)
				{
					continue;
				}
				if (deleteContainedMedia)
				{
					NavMediaSet navMediaSet = new NavMediaSet(item.ToGuid());
					IList<Guid> containedMedia = navMediaSet.GetContainedMedia();
					navRecordHandle.Target.ALDeleteAll(runTrigger: true);
					navRecordHandle.Target.ALSetFilter(1, string.Empty);
					foreach (Guid item2 in containedMedia)
					{
						navRecordHandle.Target.ALSetFilter(2, item2.ToString());
						if (!navRecordHandle.Target.ALFindFirst(DataError.TrapError))
						{
							if (list == null)
							{
								list = new List<Guid>();
							}
							list.Add(item2);
						}
					}
				}
				else
				{
					navRecordHandle.Target.ALDeleteAll(runTrigger: true);
				}
				num++;
			}
			if (list != null && list.Count > 0)
			{
				DeleteMediaObjects(parent, list, emitCallStackInLog: false);
			}
		}
		return num;
	}

	private static void LogDeletedObjectsInDiagnostics(RecordImplementation recordImplementation, IList<Guid> mediaList, bool isSet, bool emitCallStackInLog)
	{
		if (NavDiagnostics.GetMostSpecificInstance.TraceLevel >= Verbosity.Normal)
		{
			string content = string.Join(",", mediaList);
			string content2 = (emitCallStackInLog ? NavCurrentThread.Session.GetCurrentCallstack() : string.Empty);
			NavDiagnostics.GetMostSpecificInstance.SendTraceTag("0000BKC", Category.Runtime, Verbosity.Normal, "Deleting Media data referenced by record ID {0}. Number of elements {1}, isSet={2}, items={3}, callstack={4}", DiagnosticParameter.SystemMetadata(recordImplementation.InstanceIdentifier.TableId), DiagnosticParameter.SystemMetadata(mediaList.Count), DiagnosticParameter.SystemMetadata(isSet), DiagnosticParameter.SystemMetadata(content), DiagnosticParameter.SystemMetadata(content2));
		}
	}

	private static void SetCompanyFilterOnMedia(RecordImplementation recordImplementation, NavRecord mediaRecord)
	{
		if (recordImplementation != null && recordImplementation.DataPerCompany)
		{
			int companyNameToken = recordImplementation.TableState.CompanyNameToken;
			string value = ((companyNameToken > -1) ? NavCurrentThread.Session.Database.GetCompanyNameFromToken(companyNameToken) : recordImplementation.GetActiveCompany());
			int tableID = mediaRecord.TableID;
			int fieldNo = ((tableID == 2000000183 || tableID == 2000000180) ? 3 : 7);
			mediaRecord.ALSetRange(fieldNo, new NavText(value));
		}
	}

	internal static int DeleteMediaObjects(ITreeObject parent, IList<Guid> mediaIdList, bool emitCallStackInLog)
	{
		int num = 0;
		if (parent == null || mediaIdList == null || mediaIdList.Count == 0)
		{
			return 0;
		}
		RecordImplementation recordImplementation = parent as RecordImplementation;
		int mediaTableId = NavMediaHelper.GetMediaTableId(recordImplementation?.InstanceIdentifier.TableId ?? 0);
		LogDeletedObjectsInDiagnostics(recordImplementation, mediaIdList, isSet: false, emitCallStackInLog);
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { mediaTableId }))
		{
			using NavRecordHandle navRecordHandle = new NavRecordHandle(parent, mediaTableId, temporary: false, SecurityFiltering.Validated);
			SetCompanyFilterOnMedia(recordImplementation, navRecordHandle.Target);
			foreach (NavGuid item in from mediaGuid in mediaIdList.Where((Guid mediaGuid) =>
				{
					Guid guid = mediaGuid;
					return !guid.Equals(Guid.Empty);
				})
				select new NavGuid(mediaGuid))
			{
				navRecordHandle.Target.ALSetFilter(1, item.ToString());
				if (navRecordHandle.Target.ALFind(DataError.TrapError, "-"))
				{
					bool flag = true;
					if (recordImplementation != null && !recordImplementation.DataPerCompany)
					{
						flag = NavMediaHelper.IsCompanyValid(navRecordHandle.Target);
					}
					if (flag)
					{
						navRecordHandle.Target.ALDelete(runApplicationTrigger: true);
						num++;
					}
				}
			}
			return num;
		}
	}
}
