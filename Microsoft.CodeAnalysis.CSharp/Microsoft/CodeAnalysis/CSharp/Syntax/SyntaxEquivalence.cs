using System;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

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
		if (before == null || after == null)
		{
			return before == after;
		}
		return AreEquivalentRecursive(before.Green, after.Green, ignoreChildNode, topLevel);
	}

	public static bool AreEquivalent(SyntaxTokenList before, SyntaxTokenList after)
	{
		return AreEquivalentRecursive(before.Node, after.Node, null, topLevel: false);
	}

	public static bool AreEquivalent(SyntaxToken before, SyntaxToken after)
	{
		if (before.RawKind == after.RawKind)
		{
			if (before.Node != null)
			{
				return AreTokensEquivalent(before.Node, after.Node);
			}
			return true;
		}
		return false;
	}

	private static bool AreTokensEquivalent(GreenNode before, GreenNode after)
	{
		if (before.IsMissing != after.IsMissing)
		{
			return false;
		}
		switch ((SyntaxKind)(ushort)before.RawKind)
		{
		case SyntaxKind.IdentifierToken:
			return ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)before).ValueText == ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)after).ValueText;
		case SyntaxKind.NumericLiteralToken:
		case SyntaxKind.CharacterLiteralToken:
		case SyntaxKind.StringLiteralToken:
		case SyntaxKind.InterpolatedStringTextToken:
			return ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)before).Text == ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken)after).Text;
		default:
			return true;
		}
	}

	private static bool AreEquivalentRecursive(GreenNode before, GreenNode after, Func<SyntaxKind, bool> ignoreChildNode, bool topLevel)
	{
		if (before == after)
		{
			return true;
		}
		if (before == null || after == null)
		{
			return false;
		}
		if (before.RawKind != after.RawKind)
		{
			return false;
		}
		if (before.IsToken)
		{
			return AreTokensEquivalent(before, after);
		}
		if (topLevel)
		{
			SyntaxKind syntaxKind = (SyntaxKind)before.RawKind;
			if (syntaxKind == SyntaxKind.Block || syntaxKind == SyntaxKind.ArrowExpressionClause)
			{
				return true;
			}
			if ((ushort)before.RawKind == 8873)
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FieldDeclarationSyntax fieldDeclarationSyntax = (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FieldDeclarationSyntax)before;
				Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FieldDeclarationSyntax fieldDeclarationSyntax2 = (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FieldDeclarationSyntax)after;
				bool flag = fieldDeclarationSyntax.Modifiers.Any(8350);
				bool flag2 = fieldDeclarationSyntax2.Modifiers.Any(8350);
				if (!flag && !flag2)
				{
					ignoreChildNode = (SyntaxKind childKind) => childKind == SyntaxKind.EqualsValueClause;
				}
			}
		}
		if (ignoreChildNode != null)
		{
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.ChildSyntaxList.Enumerator enumerator = before.ChildNodesAndTokens().GetEnumerator();
			Microsoft.CodeAnalysis.Syntax.InternalSyntax.ChildSyntaxList.Enumerator enumerator2 = after.ChildNodesAndTokens().GetEnumerator();
			GreenNode greenNode;
			GreenNode greenNode2;
			do
			{
				greenNode = null;
				greenNode2 = null;
				while (enumerator.MoveNext())
				{
					GreenNode current = enumerator.Current;
					if (current != null && (current.IsToken || !ignoreChildNode((SyntaxKind)current.RawKind)))
					{
						greenNode = current;
						break;
					}
				}
				while (enumerator2.MoveNext())
				{
					GreenNode current2 = enumerator2.Current;
					if (current2 != null && (current2.IsToken || !ignoreChildNode((SyntaxKind)current2.RawKind)))
					{
						greenNode2 = current2;
						break;
					}
				}
				if (greenNode == null || greenNode2 == null)
				{
					return greenNode == greenNode2;
				}
			}
			while (AreEquivalentRecursive(greenNode, greenNode2, ignoreChildNode, topLevel));
			return false;
		}
		int slotCount = before.SlotCount;
		if (slotCount != after.SlotCount)
		{
			return false;
		}
		for (int num = 0; num < slotCount; num++)
		{
			GreenNode slot = before.GetSlot(num);
			GreenNode slot2 = after.GetSlot(num);
			if (!AreEquivalentRecursive(slot, slot2, ignoreChildNode, topLevel))
			{
				return false;
			}
		}
		return true;
	}
}
