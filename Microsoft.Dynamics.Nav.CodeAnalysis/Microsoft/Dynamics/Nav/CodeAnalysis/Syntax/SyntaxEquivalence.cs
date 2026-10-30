using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

internal static class SyntaxEquivalence
{
	internal static bool AreEquivalent(SyntaxTree before, SyntaxTree after, Func<SyntaxKind, bool> ignoreChildNode, bool topLevel)
	{
		if (before == after)
		{
			return true;
		}
		if (before == null || after == null)
		{
			return false;
		}
		return AreEquivalent(before.GetRoot(), after.GetRoot(), ignoreChildNode, topLevel);
	}

	public static bool AreEquivalent(SyntaxNode before, SyntaxNode after, Func<SyntaxKind, bool> ignoreChildNode, bool topLevel)
	{
		DebugAssertHelper.Assert(!topLevel || ignoreChildNode == null);
		if (before == null || after == null)
		{
			return before == after;
		}
		return AreEquivalentRecursive(before.Node, after.Node, ignoreChildNode, topLevel);
	}

	public static bool AreEquivalent(SyntaxTokenList before, SyntaxTokenList after)
	{
		return AreEquivalentRecursive(before.Node, after.Node, null, topLevel: false);
	}

	public static bool AreEquivalent(SyntaxToken before, SyntaxToken after)
	{
		if (before.Kind == after.Kind)
		{
			if (before.Node != null)
			{
				return AreTokensEquivalent(before.Node, after.Node);
			}
			return true;
		}
		return false;
	}

	[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
	private static bool AreTokensEquivalent(InternalSyntaxNode before, InternalSyntaxNode after)
	{
		DebugAssertHelper.Assert(before.Kind == after.Kind);
		if (before.IsMissing != after.IsMissing)
		{
			return false;
		}
		switch (before.Kind)
		{
		case SyntaxKind.IdentifierToken:
			return ((InternalSyntaxToken)before).ValueText == ((InternalSyntaxToken)after).ValueText;
		case SyntaxKind.Int32LiteralToken:
		case SyntaxKind.Int64LiteralToken:
		case SyntaxKind.DecimalLiteralToken:
		case SyntaxKind.DateLiteralToken:
		case SyntaxKind.TimeLiteralToken:
		case SyntaxKind.DateTimeLiteralToken:
		case SyntaxKind.StringLiteralToken:
			return ((InternalSyntaxToken)before).Text == ((InternalSyntaxToken)after).Text;
		default:
			return true;
		}
	}

	private static bool AreEquivalentRecursive(InternalSyntaxNode before, InternalSyntaxNode after, Func<SyntaxKind, bool> ignoreChildNode, bool topLevel)
	{
		if (before == after)
		{
			return true;
		}
		if (before == null || after == null)
		{
			return false;
		}
		if (before.Kind != after.Kind)
		{
			return false;
		}
		if (before.IsToken)
		{
			DebugAssertHelper.Assert(after.IsToken);
			return AreTokensEquivalent(before, after);
		}
		if (topLevel && before.Kind == SyntaxKind.Block)
		{
			return true;
		}
		if (ignoreChildNode != null)
		{
			InternalChildSyntaxList.Enumerator enumerator = before.ChildNodesAndTokens().GetEnumerator();
			InternalChildSyntaxList.Enumerator enumerator2 = after.ChildNodesAndTokens().GetEnumerator();
			InternalSyntaxNode internalSyntaxNode;
			InternalSyntaxNode internalSyntaxNode2;
			do
			{
				internalSyntaxNode = null;
				internalSyntaxNode2 = null;
				while (enumerator.MoveNext())
				{
					InternalSyntaxNode current = enumerator.Current;
					if (current != null && (current.IsToken || !ignoreChildNode(current.Kind)))
					{
						internalSyntaxNode = current;
						break;
					}
				}
				while (enumerator2.MoveNext())
				{
					InternalSyntaxNode current2 = enumerator2.Current;
					if (current2 != null && (current2.IsToken || !ignoreChildNode(current2.Kind)))
					{
						internalSyntaxNode2 = current2;
						break;
					}
				}
				if (internalSyntaxNode == null || internalSyntaxNode2 == null)
				{
					return internalSyntaxNode == internalSyntaxNode2;
				}
			}
			while (AreEquivalentRecursive(internalSyntaxNode, internalSyntaxNode2, ignoreChildNode, topLevel));
			return false;
		}
		int slotCount = before.SlotCount;
		if (slotCount != after.SlotCount)
		{
			return false;
		}
		for (int i = 0; i < slotCount; i++)
		{
			InternalSyntaxNode slot = before.GetSlot(i);
			InternalSyntaxNode slot2 = after.GetSlot(i);
			if (!AreEquivalentRecursive(slot, slot2, ignoreChildNode, topLevel))
			{
				return false;
			}
		}
		return true;
	}
}
