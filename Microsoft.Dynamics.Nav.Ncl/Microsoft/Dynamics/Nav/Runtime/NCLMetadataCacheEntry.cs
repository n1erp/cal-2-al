using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetadataCacheEntry
{
	private readonly object lockObj = new object();

	private Dictionary<int, NavAppObjectEntry> appObjects;

	private bool allAppObjectsHaveNoGroupObject = true;

	public ApplicationObjectId ObjectId { get; }

	public NCLMetaApplicationObject BaseObject { get; }

	public bool BaseObjectIsStale
	{
		get
		{
			if (BaseObject != null)
			{
				return BaseObject.Stale;
			}
			return false;
		}
	}

	public bool ShouldTryLazyLoadingBase
	{
		get
		{
			bool baseObjectIsStale = BaseObjectIsStale;
			return baseObjectIsStale | (BaseObject == null && appObjects != null && allAppObjectsHaveNoGroupObject);
		}
	}

	public IEnumerable<NavAppObjectEntry> AppObjects
	{
		get
		{
			lock (lockObj)
			{
				if (appObjects == null)
				{
					return null;
				}
				return appObjects.Values.OrderBy((NavAppObjectEntry v) => v.GroupId).ToList();
			}
		}
	}

	private NCLMetadataCacheEntry(ApplicationObjectId objectId, NCLMetaApplicationObject baseObject, NavAppObjectEntry[] appEntries)
	{
		ObjectId = objectId;
		BaseObject = baseObject;
		if (appEntries != null)
		{
			appObjects = new Dictionary<int, NavAppObjectEntry>();
			for (int i = 0; i < appEntries.Length; i++)
			{
				NavAppObjectEntry value = appEntries[i];
				appObjects.Add(value.GroupId, value);
				allAppObjectsHaveNoGroupObject &= value.GroupObject == null;
			}
		}
	}

	public static NCLMetadataCacheEntry CreateWithNoBase(ApplicationObjectId objectId)
	{
		return new NCLMetadataCacheEntry(objectId, null, null);
	}

	public static NCLMetadataCacheEntry CreateWithBase(NCLMetaApplicationObject baseObject)
	{
		if (baseObject == null)
		{
			throw new ArgumentNullException("baseObject");
		}
		return new NCLMetadataCacheEntry(baseObject.ApplicationObjectId, baseObject, null);
	}

	public static NCLMetadataCacheEntry CreateWithExistingAppEntries(NCLMetaApplicationObject baseObject, NCLMetadataCacheEntry existingEntryWithAppObjects)
	{
		if (baseObject == null)
		{
			throw new ArgumentNullException("baseObject");
		}
		return new NCLMetadataCacheEntry(appEntries: (existingEntryWithAppObjects?.appObjects)?.Values.ToArray(), objectId: baseObject.ApplicationObjectId, baseObject: baseObject);
	}

	public void AddNavAppObject(int groupId, int metadataGroupId, NCLMetaApplicationObject navAppObject)
	{
		if (groupId <= 0)
		{
			throw new ArgumentOutOfRangeException("groupId");
		}
		lock (lockObj)
		{
			appObjects = appObjects ?? new Dictionary<int, NavAppObjectEntry>();
			appObjects.Add(groupId, new NavAppObjectEntry(groupId, metadataGroupId, navAppObject));
			allAppObjectsHaveNoGroupObject &= navAppObject == null;
		}
	}

	public void RemoveGroupObjectIfPresent(int groupId)
	{
		lock (lockObj)
		{
			if (appObjects != null && appObjects.Remove(groupId))
			{
				if (appObjects.Count == 0)
				{
					appObjects = null;
					allAppObjectsHaveNoGroupObject = true;
				}
				else
				{
					RefreshAppObjectsGroupObjectStates();
				}
			}
		}
	}

	public NCLMetaApplicationObject SpecificGroupObject(int groupId)
	{
		groupId = ResolveGroupId(groupId);
		if (groupId == 0)
		{
			return BaseObject;
		}
		lock (lockObj)
		{
			if (appObjects == null)
			{
				return null;
			}
			NavAppObjectEntry value;
			return appObjects.TryGetValue(groupId, out value) ? value.GroupObject : null;
		}
	}

	public static int ResolveGroupId(int groupId)
	{
		if (groupId == -1)
		{
			groupId = NavCurrentThread.ResolveAppGroup().GroupId;
		}
		return groupId;
	}

	public NCLMetaApplicationObject SpecificGroupOrBaseObject(int groupId)
	{
		return SpecificGroupObject(groupId) ?? BaseObject;
	}

	public void MarkEverythingAsStale()
	{
		if (BaseObject != null)
		{
			BaseObject.Stale = true;
		}
		lock (lockObj)
		{
			if (appObjects == null)
			{
				return;
			}
			foreach (NavAppObjectEntry value in appObjects.Values)
			{
				if (value.GroupObject != null)
				{
					value.GroupObject.Stale = true;
				}
			}
		}
	}

	public void ReplaceSingleGroupObject(int groupId, NCLMetaApplicationObject newGroupObject)
	{
		lock (lockObj)
		{
			if (appObjects != null && appObjects.TryGetValue(groupId, out var value))
			{
				appObjects[groupId] = new NavAppObjectEntry(groupId, value.MetadataGroupId, newGroupObject);
				if (newGroupObject != null)
				{
					allAppObjectsHaveNoGroupObject = false;
				}
				else if (value.GroupObject != null)
				{
					RefreshAppObjectsGroupObjectStates();
				}
			}
		}
	}

	private void RefreshAppObjectsGroupObjectStates()
	{
		allAppObjectsHaveNoGroupObject = true;
		foreach (KeyValuePair<int, NavAppObjectEntry> appObject in appObjects)
		{
			allAppObjectsHaveNoGroupObject &= appObject.Value.GroupObject == null;
		}
	}
}
