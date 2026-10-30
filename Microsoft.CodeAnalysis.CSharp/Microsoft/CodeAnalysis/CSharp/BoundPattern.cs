namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundPattern : BoundNode
{
	protected BoundPattern(BoundKind kind, SyntaxNode syntax, bool hasErrors)
		: base(kind, syntax, hasErrors)
	{
	}

	protected BoundPattern(BoundKind kind, SyntaxNode syntax)
		: base(kind, syntax)
	{
	}
}
