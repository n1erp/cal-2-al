using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal struct InternalSeparatedSyntaxList<TNode> : IEquatable<InternalSeparatedSyntaxList<TNode>> where TNode : InternalSyntaxNode
{
	private readonly InternalSyntaxList<InternalSyntaxNode> list;

	internal InternalSyntaxNode Node => list.Node;

	public int Count => list.Count + 1 >> 1;

	public int SeparatorCount => list.Count >> 1;

	public TNode this[int index] => (TNode)list[index << 1];

	internal InternalSeparatedSyntaxList(InternalSyntaxList<InternalSyntaxNode> list)
	{
		this.list = list;
	}

	[Conditional("DEBUG")]
	private static void Validate(InternalSyntaxList<InternalSyntaxNode> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			InternalSyntaxToken internalSyntaxToken = list[i] as InternalSyntaxToken;
			if ((i & 1) == 0)
			{
				DebugAssertHelper.Assert(internalSyntaxToken == null, "even elements of a separated list must be nodes");
			}
			else
			{
				DebugAssertHelper.Assert(internalSyntaxToken != null, "odd elements of a separated list must be tokens");
			}
		}
	}

	public InternalSyntaxToken GetSeparator(int index)
	{
		return (InternalSyntaxToken)list[(index << 1) + 1];
	}

	public InternalSyntaxList<InternalSyntaxNode> GetWithSeparators()
	{
		return list;
	}

	public static bool operator ==(InternalSeparatedSyntaxList<TNode> left, InternalSeparatedSyntaxList<TNode> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(InternalSeparatedSyntaxList<TNode> left, InternalSeparatedSyntaxList<TNode> right)
	{
		return !left.Equals(right);
	}

	public bool Equals(InternalSeparatedSyntaxList<TNode> other)
	{
		return list == other.list;
	}

	public override bool Equals(object obj)
	{
		if (obj is InternalSeparatedSyntaxList<TNode>)
		{
			return Equals((InternalSeparatedSyntaxList<TNode>)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return list.GetHashCode();
	}
}
