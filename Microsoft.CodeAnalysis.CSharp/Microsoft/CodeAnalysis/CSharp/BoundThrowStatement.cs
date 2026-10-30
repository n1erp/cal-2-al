namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundThrowStatement : BoundStatement
{
	public BoundExpression ExpressionOpt { get; }

	public BoundThrowStatement(SyntaxNode syntax, BoundExpression expressionOpt, bool hasErrors = false)
		: base(BoundKind.ThrowStatement, syntax, hasErrors || expressionOpt.HasErrors())
	{
		ExpressionOpt = expressionOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitThrowStatement(this);
	}

	public BoundThrowStatement Update(BoundExpression expressionOpt)
	{
		if (expressionOpt != ExpressionOpt)
		{
			return new BoundThrowStatement(Syntax, expressionOpt, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
