using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NavBookmarkChangeTracking : ICloneable, IExtensibleDataObject
{
	private int tableId;

	private bool isTemporary;

	private Guid[] formHandles;

	private bool manyChanges;

	private NavBookmarkUpdateOperation[] changes;

	[NonSerialized]
	private ExtensionDataObject unknownDataBag;

	public virtual ExtensionDataObject ExtensionData
	{
		get
		{
			return unknownDataBag;
		}
		set
		{
			unknownDataBag = value;
		}
	}

	[DataMember]
	public int TableId
	{
		[DebuggerStepThrough]
		get
		{
			return tableId;
		}
		[DebuggerStepThrough]
		set
		{
			tableId = value;
		}
	}

	[DataMember]
	public bool IsTemporary
	{
		[DebuggerStepThrough]
		get
		{
			return isTemporary;
		}
		[DebuggerStepThrough]
		set
		{
			isTemporary = value;
		}
	}

	[DataMember]
	public Guid[] FormHandles
	{
		[DebuggerStepThrough]
		get
		{
			return formHandles;
		}
		[DebuggerStepThrough]
		set
		{
			formHandles = value;
		}
	}

	[DataMember]
	public bool ManyChanges
	{
		[DebuggerStepThrough]
		get
		{
			return manyChanges;
		}
		[DebuggerStepThrough]
		set
		{
			manyChanges = value;
		}
	}

	[DataMember]
	public NavBookmarkUpdateOperation[] Changes
	{
		[DebuggerStepThrough]
		get
		{
			return changes;
		}
		[DebuggerStepThrough]
		set
		{
			changes = value;
		}
	}

	public object Clone()
	{
		NavBookmarkChangeTracking navBookmarkChangeTracking = new NavBookmarkChangeTracking();
		navBookmarkChangeTracking.tableId = tableId;
		navBookmarkChangeTracking.isTemporary = isTemporary;
		navBookmarkChangeTracking.manyChanges = manyChanges;
		if (changes == null)
		{
			navBookmarkChangeTracking.Changes = null;
		}
		else
		{
			navBookmarkChangeTracking.changes = (NavBookmarkUpdateOperation[])changes.Clone();
		}
		if (formHandles == null)
		{
			navBookmarkChangeTracking.formHandles = null;
		}
		else
		{
			navBookmarkChangeTracking.formHandles = (Guid[])formHandles.Clone();
		}
		return navBookmarkChangeTracking;
	}
}
