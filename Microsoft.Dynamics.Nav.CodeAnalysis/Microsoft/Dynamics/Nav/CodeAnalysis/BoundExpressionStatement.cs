using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundExpressionStatement : BoundStatement, IExpressionStatement, IOperation
{
	public BoundExpression Expression { get; private set; }

	IOperation IExpressionStatement.Expression => Expression;

	protected override OperationKind StatementKind => OperationKind.ExpressionStatement;

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

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitExpressionStatement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitExpressionStatement(this, argument);
	}
}
