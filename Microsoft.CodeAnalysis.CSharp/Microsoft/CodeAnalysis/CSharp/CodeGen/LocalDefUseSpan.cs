namespace Microsoft.CodeAnalysis.CSharp.CodeGen;

internal struct LocalDefUseSpan
{
	public readonly int Start;

	public readonly int End;

	public LocalDefUseSpan(int start)
		: this(start, start)
	{
	}

	private LocalDefUseSpan(int start, int end)
	{
		Start = start;
		End = end;
	}

	internal LocalDefUseSpan WithEnd(int end)
	{
		return new LocalDefUseSpan(Start, end);
	}

	public override string ToString()
	{
		return "[" + Start + " ," + End + ")";
	}

	public bool ConflictsWith(LocalDefUseSpan other)
	{
		return Contains(other.Start) ^ Contains(other.End);
	}

	private bool Contains(int val)
	{
		if (Start < val)
		{
			return End > val;
		}
		return false;
	}

	public bool ConflictsWithDummy(LocalDefUseSpan dummy)
	{
		return Includes(dummy.Start) ^ Includes(dummy.End);
	}

	private bool Includes(int val)
	{
		if (Start <= val)
		{
			return End >= val;
		}
		return false;
	}
}
