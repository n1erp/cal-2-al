namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundWildcardPattern : BoundPattern
{
	public BoundWildcardPattern(SyntaxNode syntax, bool hasErrors)
		: base(BoundKind.WildcardPattern, syntax, hasErrors)
	{
	}

	public BoundWildcardPattern(SyntaxNode syntax)
		: base(BoundKind.WildcardPattern, syntax)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitWildcardPattern(this);
	}
}
