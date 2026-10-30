namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

internal static class SyntaxListBuilderExtensions
{
	public static SyntaxList<SyntaxNode> ToList(this SyntaxListBuilder builder)
	{
		if (builder == null || builder.Count == 0)
		{
			return default;
		}
		return new SyntaxList<SyntaxNode>(builder.ToListNode().CreateRed());
	}

	public static SeparatedSyntaxList<TNode> ToSeparatedList<TNode>(this SyntaxListBuilder builder) where TNode : SyntaxNode
	{
		if (builder == null || builder.Count == 0)
		{
			return default;
		}
		return new SeparatedSyntaxList<TNode>(new SyntaxNodeOrTokenList(builder.ToListNode().CreateRed(), 0));
	}
}
