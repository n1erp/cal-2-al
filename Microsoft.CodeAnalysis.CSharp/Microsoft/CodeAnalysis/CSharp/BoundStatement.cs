namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundStatement : BoundNode
{
	protected BoundStatement(BoundKind kind, SyntaxNode syntax, bool hasErrors)
		: base(kind, syntax, hasErrors)
	{
	}

	protected BoundStatement(BoundKind kind, SyntaxNode syntax)
		: base(kind, syntax)
	{
	}
}
