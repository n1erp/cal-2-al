namespace Microsoft.CodeAnalysis.Syntax.InternalSyntax;

internal static class GreenNodeExtensions
{
	internal static SyntaxList<T> ToGreenList<T>(this SyntaxNode node) where T : GreenNode
	{
		return node?.Green.ToGreenList<T>() ?? default(SyntaxList<T>);
	}

	internal static SeparatedSyntaxList<T> ToGreenSeparatedList<T>(this SyntaxNode node) where T : GreenNode
	{
		if (node == null)
		{
			return default;
		}
		return new SeparatedSyntaxList<T>(node.Green.ToGreenList<T>());
	}

	internal static SyntaxList<T> ToGreenList<T>(this GreenNode node) where T : GreenNode
	{
		return new SyntaxList<T>(node);
	}
}
