using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.MetaModel;

internal class MergeChange : IMergeChange
{
	private MergeChangeType changeType;

	private readonly List<IDiffChange> modifiedChanges;

	private readonly List<IDiffChange> targetChanges;

	private IDiffChange targetChange;

	private IDiffChange modifiedChange;

	public MergeChangeType ChangeType => changeType;

	public IDiffChange[] ModifiedChanges => modifiedChanges.ToArray();

	public IDiffChange[] TargetChanges => targetChanges.ToArray();

	public IDiffChange ModifiedChange
	{
		get
		{
			if (modifiedChange == null)
			{
				modifiedChange = BuildConsolidatedChange(modifiedChanges);
			}
			return modifiedChange;
		}
	}

	public IDiffChange TargetChange
	{
		get
		{
			if (targetChange == null)
			{
				targetChange = BuildConsolidatedChange(targetChanges);
			}
			return targetChange;
		}
	}

	public MergeChange(MergeChangeType changeType)
	{
		this.changeType = changeType;
		modifiedChanges = new List<IDiffChange>();
		targetChanges = new List<IDiffChange>();
	}

	public MergeChange(MergeChangeType changeType, IDiffChange modifiedChange, IDiffChange targetChange)
	{
		this.changeType = changeType;
		modifiedChanges = new List<IDiffChange>();
		targetChanges = new List<IDiffChange>();
		if (modifiedChange != null)
		{
			modifiedChanges.Add(modifiedChange);
		}
		if (targetChange != null)
		{
			targetChanges.Add(targetChange);
		}
	}

	public void AddModifiedChange(IDiffChange newModifiedChange)
	{
		modifiedChanges.Add(newModifiedChange);
		modifiedChange = null;
		if (targetChanges.Count > 0)
		{
			changeType = MergeChangeType.Conflict;
		}
	}

	public void AddTargetChange(IDiffChange newTargetChange)
	{
		targetChanges.Add(newTargetChange);
		targetChange = null;
		if (modifiedChanges.Count > 0)
		{
			changeType = MergeChangeType.Conflict;
		}
	}

	private static IDiffChange BuildConsolidatedChange(List<IDiffChange> diffChanges)
	{
		if (diffChanges.Count == 0)
		{
			return null;
		}
		if (diffChanges.Count == 1)
		{
			return diffChanges[0];
		}
		IDiffChange consolidatedDiffChange = diffChanges[0];
		diffChanges.ForEach((IDiffChange diffChange) =>
		{
			consolidatedDiffChange = consolidatedDiffChange.Add(diffChange);
		});
		return consolidatedDiffChange;
	}
}
