namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class SyntaxLastTokenReplacer : CSharpSyntaxRewriter
{
	private readonly SyntaxToken _oldToken;

	private readonly SyntaxToken _newToken;

	private int _count = 1;

	private bool _found;

	private SyntaxLastTokenReplacer(SyntaxToken oldToken, SyntaxToken newToken)
	{
		_oldToken = oldToken;
		_newToken = newToken;
	}

	internal static TRoot Replace<TRoot>(TRoot root, SyntaxToken newToken) where TRoot : CSharpSyntaxNode
	{
		return (TRoot)new SyntaxLastTokenReplacer(root.GetLastToken(), newToken).Visit(root);
	}

	private static int CountNonNullSlots(CSharpSyntaxNode node)
	{
		return node.ChildNodesAndTokens().Count;
	}

	public override CSharpSyntaxNode Visit(CSharpSyntaxNode node)
	{
		if (node != null && !_found)
		{
			_count--;
			if (_count == 0)
			{
				if (node is SyntaxToken)
				{
					_found = true;
					return _newToken;
				}
				_count += CountNonNullSlots(node);
				return base.Visit(node);
			}
		}
		return node;
	}
}
