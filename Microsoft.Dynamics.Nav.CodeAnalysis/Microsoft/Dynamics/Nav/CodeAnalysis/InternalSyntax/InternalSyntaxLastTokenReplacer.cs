namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class InternalSyntaxLastTokenReplacer : InternalSyntaxRewriter
{
	private readonly InternalSyntaxToken oldToken;

	private readonly InternalSyntaxToken newToken;

	private int count = 1;

	private bool found;

	private InternalSyntaxLastTokenReplacer(InternalSyntaxToken oldToken, InternalSyntaxToken newToken)
	{
		this.oldToken = oldToken;
		this.newToken = newToken;
	}

	internal static TRoot Replace<TRoot>(TRoot root, InternalSyntaxToken newToken) where TRoot : InternalSyntaxNode
	{
		InternalSyntaxLastTokenReplacer internalSyntaxLastTokenReplacer = new InternalSyntaxLastTokenReplacer(root.GetLastToken(), newToken);
		TRoot result = (TRoot)internalSyntaxLastTokenReplacer.Visit(root);
		DebugAssertHelper.Assert(internalSyntaxLastTokenReplacer.found);
		return result;
	}

	private static int CountNonNullSlots(InternalSyntaxNode node)
	{
		return node.ChildNodesAndTokens().Count;
	}

	public override InternalSyntaxNode Visit(InternalSyntaxNode node)
	{
		if (node != null && !found)
		{
			count--;
			if (count == 0)
			{
				if (node is InternalSyntaxToken internalSyntaxToken)
				{
					DebugAssertHelper.Assert(internalSyntaxToken == oldToken);
					found = true;
					return newToken;
				}
				count += CountNonNullSlots(node);
				return base.Visit(node);
			}
		}
		return node;
	}
}
