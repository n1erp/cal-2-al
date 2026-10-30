namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundExpressionStatement : BoundStatement
{
	public BoundExpression Expression { get; }

	public BoundExpressionStatement(SyntaxNode syntax, BoundExpression expression, bool hasErrors = false)
		: base(BoundKind.ExpressionStatement, syntax, hasErrors || expression.HasErrors())
	{
		Expression = expression;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitExpressionStatement(this);
	}

	public BoundExpressionStatement Update(BoundExpression expression)
	{
		if (expression != Expression)
		{
			return new BoundExpressionStatement(Syntax, expression, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
