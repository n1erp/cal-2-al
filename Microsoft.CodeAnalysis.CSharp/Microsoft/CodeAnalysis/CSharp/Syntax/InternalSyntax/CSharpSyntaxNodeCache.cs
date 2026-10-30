using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal static class CSharpSyntaxNodeCache
{
	internal static GreenNode TryGetNode(int kind, GreenNode child1, SyntaxFactoryContext context, out int hash)
	{
		return SyntaxNodeCache.TryGetNode(kind, child1, GetNodeFlags(context), out hash);
	}

	internal static GreenNode TryGetNode(int kind, GreenNode child1, GreenNode child2, SyntaxFactoryContext context, out int hash)
	{
		return SyntaxNodeCache.TryGetNode(kind, child1, child2, GetNodeFlags(context), out hash);
	}

	internal static GreenNode TryGetNode(int kind, GreenNode child1, GreenNode child2, GreenNode child3, SyntaxFactoryContext context, out int hash)
	{
		return SyntaxNodeCache.TryGetNode(kind, child1, child2, child3, GetNodeFlags(context), out hash);
	}

	private static GreenNode.NodeFlags GetNodeFlags(SyntaxFactoryContext context)
	{
		GreenNode.NodeFlags nodeFlags = SyntaxNodeCache.GetDefaultNodeFlags();
		if (context.IsInAsync)
		{
			nodeFlags |= GreenNode.NodeFlags.FactoryContextIsInAsync;
		}
		if (context.IsInQuery)
		{
			nodeFlags |= GreenNode.NodeFlags.FactoryContextIsInQuery;
		}
		return nodeFlags;
	}
}
