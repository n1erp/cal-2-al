using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.MetaModel;

internal class DiffChangeHelper : IDisposable
{
	private int originalCount;

	private int modifiedCount;

	private List<IDiffChange> changes;

	private int originalStart;

	private int modifiedStart;

	public IDiffChange[] Changes
	{
		get
		{
			if (originalCount > 0 || modifiedCount > 0)
			{
				MarkNextChange();
			}
			return changes.ToArray();
		}
	}

	public IDiffChange[] ReverseChanges
	{
		get
		{
			if (originalCount > 0 || modifiedCount > 0)
			{
				MarkNextChange();
			}
			changes.Reverse();
			return changes.ToArray();
		}
	}

	public DiffChangeHelper()
	{
		changes = new List<IDiffChange>();
		originalStart = int.MaxValue;
		modifiedStart = int.MaxValue;
	}

	public void Dispose()
	{
		changes = null;
		GC.SuppressFinalize(this);
	}

	public void MarkNextChange()
	{
		if (originalCount > 0 || modifiedCount > 0)
		{
			changes.Add(new DiffChange(originalStart, originalCount, modifiedStart, modifiedCount));
		}
		originalCount = 0;
		modifiedCount = 0;
		originalStart = int.MaxValue;
		modifiedStart = int.MaxValue;
	}

	public void AddOriginalElement(int originalIndex, int modifiedIndex)
	{
		originalStart = Math.Min(originalStart, originalIndex);
		modifiedStart = Math.Min(modifiedStart, modifiedIndex);
		originalCount++;
	}

	public void AddModifiedElement(int originalIndex, int modifiedIndex)
	{
		originalStart = Math.Min(originalStart, originalIndex);
		modifiedStart = Math.Min(modifiedStart, modifiedIndex);
		modifiedCount++;
	}
}
