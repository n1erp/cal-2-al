namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

public static class ChangeKindExtensions
{
	public static bool IsMove(this ChangeKind kind)
	{
		if (kind != ChangeKind.MoveAfter && kind != ChangeKind.MoveBefore && kind != ChangeKind.MoveFirst)
		{
			return kind == ChangeKind.MoveLast;
		}
		return true;
	}

	public static bool IsAdd(this ChangeKind kind)
	{
		if (kind != ChangeKind.Add && kind != ChangeKind.AddAfter && kind != ChangeKind.AddBefore && kind != ChangeKind.AddFirst)
		{
			return kind == ChangeKind.AddLast;
		}
		return true;
	}

	public static bool IsAddToContainer(this ChangeKind kind)
	{
		if (kind != ChangeKind.AddFirst)
		{
			return kind == ChangeKind.AddLast;
		}
		return true;
	}

	public static bool IsModify(this ChangeKind kind)
	{
		return kind == ChangeKind.Modify;
	}

	public static bool IsAbsoluteInAnchor(this ChangeKind kind)
	{
		if (kind != ChangeKind.AddFirst && kind != ChangeKind.AddLast && kind != ChangeKind.MoveFirst)
		{
			return kind == ChangeKind.MoveLast;
		}
		return true;
	}

	public static bool IsRelativeToAnchor(this ChangeKind kind)
	{
		if (kind != ChangeKind.AddBefore && kind != ChangeKind.AddAfter && kind != ChangeKind.MoveBefore)
		{
			return kind == ChangeKind.MoveAfter;
		}
		return true;
	}
}
