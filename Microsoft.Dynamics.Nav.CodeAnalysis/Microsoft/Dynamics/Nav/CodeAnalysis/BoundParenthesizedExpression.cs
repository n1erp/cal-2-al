using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundParenthesizedExpression : BoundExpression, IParenthesizedExpression, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override ConstantValue ConstantValue => Expression.ConstantValue;

	public override Symbol ExpressionSymbol => Type;

	public BoundExpression Expression { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	IOperation IParenthesizedExpression.Operand => Expression;

	protected override OperationKind ExpressionKind => OperationKind.ParenthesizedExpression;

	public BoundParenthesizedExpression(SyntaxNode syntax, BoundExpression expression, LookupResultKind resultKind, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ParenthesizedExpression, syntax, type, hasErrors || expression.HasErrors())
	{
		Expression = expression;
		_ResultKind = resultKind;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitParenthesizedExpression(this);
	}

	public BoundParenthesizedExpression Update(BoundExpression expression, LookupResultKind resultKind, TypeSymbol type)
	{
		if (expression != Expression || resultKind != ResultKind || type != Type)
		{
			return new BoundParenthesizedExpression(Syntax, expression, resultKind, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitParenthesizedExpression(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitParenthesizedExpression(this, argument);
	}
}
