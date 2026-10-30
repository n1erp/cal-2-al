namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundInitializer : BoundNode
{
	protected BoundInitializer(BoundKind kind, SyntaxNode syntax, bool hasErrors)
		: base(kind, syntax, hasErrors)
	{
	}

	protected BoundInitializer(BoundKind kind, SyntaxNode syntax)
		: base(kind, syntax)
	{
	}
}
