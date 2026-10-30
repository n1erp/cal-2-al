using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavTableChangeTracking : TreeObject
{
	internal const int MaxNumberOfChangesToTrack = 5;

	private bool manyChanges;

	private List<Guid> formHandles;

	private List<NavBookmarkUpdateOperation> changes;

	internal List<NavBookmarkUpdateOperation> Changes => changes;

	internal List<Guid> FormHandles => formHandles;

	internal bool ManyChanges
	{
		get
		{
			return manyChanges;
		}
		set
		{
			manyChanges = value;
		}
	}

	internal NavTableChangeTracking(ITreeObject parent)
		: base(parent)
	{
		formHandles = new List<Guid>();
		changes = new List<NavBookmarkUpdateOperation>(5);
	}

	internal void Add(NavRecordOperationTypes operation, NavRecord record, NavRecord newRecord)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (ManyChanges)
		{
			return;
		}
		if (Changes.Count >= 5 || (operation & NavRecordOperationTypes.Bulk) != 0)
		{
			ManyChanges = true;
			Changes.Clear();
			return;
		}
		NavBookmarkUpdateOperation item = new NavBookmarkUpdateOperation
		{
			Operation = operation
		};
		NavRecordId aLRecordId = record.ALRecordId;
		item.RecordId = aLRecordId.GetBytes();
		if (newRecord != null)
		{
			NavRecordId aLRecordId2 = newRecord.ALRecordId;
			item.NewRecordId = aLRecordId2.GetBytes();
		}
		else
		{
			item.NewRecordId = null;
		}
		Changes.Add(item);
	}
}
