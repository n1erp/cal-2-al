using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTrackChanges : TreeObject
{
	private Dictionary<NavTableInstanceIdentifier, NavTableChangeTracking> trackedChanges;

	internal NavTrackChanges(NavCompany parent)
		: base(parent)
	{
		trackedChanges = new Dictionary<NavTableInstanceIdentifier, NavTableChangeTracking>();
	}

	public NavBookmarkChangeTracking[] GetAndClearChanges()
	{
		List<NavBookmarkChangeTracking> list = null;
		if (trackedChanges != null)
		{
			lock (trackedChanges)
			{
				if (trackedChanges.Count > 0)
				{
					foreach (KeyValuePair<NavTableInstanceIdentifier, NavTableChangeTracking> trackedChange in trackedChanges)
					{
						NavTableInstanceIdentifier key = trackedChange.Key;
						NavTableChangeTracking value = trackedChange.Value;
						if (value.ManyChanges || value.Changes.Count > 0)
						{
							NavBookmarkChangeTracking navBookmarkChangeTracking = new NavBookmarkChangeTracking();
							navBookmarkChangeTracking.TableId = key.TableId;
							navBookmarkChangeTracking.IsTemporary = key.IsTemporary;
							navBookmarkChangeTracking.FormHandles = ((value.FormHandles != null) ? value.FormHandles.ToArray() : null);
							navBookmarkChangeTracking.Changes = ((value.Changes != null) ? value.Changes.ToArray() : null);
							navBookmarkChangeTracking.ManyChanges = value.ManyChanges;
							if (list == null)
							{
								list = new List<NavBookmarkChangeTracking>();
							}
							list.Add(navBookmarkChangeTracking);
							value.Changes.Clear();
							value.ManyChanges = false;
						}
					}
				}
			}
		}
		return list?.ToArray();
	}

	internal void TrackChange(NavRecordOperationTypes operation, NavRecord record, NavRecord newRecord)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (trackedChanges == null)
		{
			return;
		}
		NavTableInstanceIdentifier instanceIdentifier = record.InstanceIdentifier;
		lock (trackedChanges)
		{
			if (trackedChanges.TryGetValue(instanceIdentifier, out var value))
			{
				value.Add(operation, record, newRecord);
			}
		}
	}

	internal void RegisterFormTableChanges(Guid formIdentifier, NavTableInstanceIdentifier tableInstanceIdentifier)
	{
		lock (trackedChanges)
		{
			if (!trackedChanges.TryGetValue(tableInstanceIdentifier, out var value))
			{
				value = new NavTableChangeTracking(this);
				trackedChanges.Add(tableInstanceIdentifier, value);
			}
			value.FormHandles.Add(formIdentifier);
		}
	}

	internal void UnregisterFormTableChanges(Guid formIdentifier, NavTableInstanceIdentifier tableInstanceIdentifier)
	{
		lock (trackedChanges)
		{
			if (trackedChanges.TryGetValue(tableInstanceIdentifier, out var value) && value.FormHandles.Remove(formIdentifier) && value.FormHandles.Count == 0)
			{
				trackedChanges.Remove(tableInstanceIdentifier);
				value.Dispose();
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		trackedChanges = null;
		base.Dispose(disposing);
	}
}
