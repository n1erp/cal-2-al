namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundYieldBreakStatement : BoundStatement
{
	public static BoundYieldBreakStatement Synthesized(SyntaxNode syntax, bool hasErrors = false)
	{
		return new BoundYieldBreakStatement(syntax, hasErrors)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundYieldBreakStatement(SyntaxNode syntax, bool hasErrors)
		: base(BoundKind.YieldBreakStatement, syntax, hasErrors)
	{
	}

	public BoundYieldBreakStatement(SyntaxNode syntax)
		: base(BoundKind.YieldBreakStatement, syntax)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitYieldBreakStatement(this);
	}
}
