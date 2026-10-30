namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundYieldReturnStatement : BoundStatement
{
	public BoundExpression Expression { get; }

	public BoundYieldReturnStatement(SyntaxNode syntax, BoundExpression expression, bool hasErrors = false)
		: base(BoundKind.YieldReturnStatement, syntax, hasErrors || expression.HasErrors())
	{
		Expression = expression;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitYieldReturnStatement(this);
	}

	public BoundYieldReturnStatement Update(BoundExpression expression)
	{
		if (expression != Expression)
		{
			return new BoundYieldReturnStatement(Syntax, expression, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
