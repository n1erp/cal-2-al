using System;

namespace Microsoft.Dynamics.Nav.MetaModel;

internal class DiffChange : IDiffChange
{
	private DiffChangeType changeType;

	private int originalStart;

	private int originalLength;

	private int modifiedStart;

	private int modifiedLength;

	private bool updateChangeType;

	public DiffChangeType ChangeType
	{
		get
		{
			if (updateChangeType)
			{
				UpdateChangeType();
			}
			return changeType;
		}
	}

	public int OriginalStart
	{
		get
		{
			return originalStart;
		}
		set
		{
			originalStart = value;
			updateChangeType = true;
		}
	}

	public int OriginalLength
	{
		get
		{
			return originalLength;
		}
		set
		{
			originalLength = value;
			updateChangeType = true;
		}
	}

	public int OriginalEnd => OriginalStart + OriginalLength;

	public int ModifiedStart
	{
		get
		{
			return modifiedStart;
		}
		set
		{
			modifiedStart = value;
			updateChangeType = true;
		}
	}

	public int ModifiedLength
	{
		get
		{
			return modifiedLength;
		}
		set
		{
			modifiedLength = value;
			updateChangeType = true;
		}
	}

	public int ModifiedEnd => ModifiedStart + ModifiedLength;

	internal DiffChange(int originalStart, int originalLength, int modifiedStart, int modifiedLength)
	{
		this.originalStart = originalStart;
		this.originalLength = originalLength;
		this.modifiedStart = modifiedStart;
		this.modifiedLength = modifiedLength;
		UpdateChangeType();
	}

	private void UpdateChangeType()
	{
		if (originalLength > 0)
		{
			changeType = ((modifiedLength <= 0) ? DiffChangeType.Delete : DiffChangeType.Change);
		}
		else if (modifiedLength > 0)
		{
			changeType = DiffChangeType.Insert;
		}
		updateChangeType = false;
	}

	public IDiffChange Add(IDiffChange diffChange)
	{
		if (diffChange == null)
		{
			return this;
		}
		int num = Math.Min(OriginalStart, diffChange.OriginalStart);
		int num2 = Math.Max(OriginalEnd, diffChange.OriginalEnd);
		int num3 = Math.Min(ModifiedStart, diffChange.ModifiedStart);
		int num4 = Math.Max(ModifiedEnd, diffChange.ModifiedEnd);
		return new DiffChange(num, num2 - num, num3, num4 - num3);
	}
}
