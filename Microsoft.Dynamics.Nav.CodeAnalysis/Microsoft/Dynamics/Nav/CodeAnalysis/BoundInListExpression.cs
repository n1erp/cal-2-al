using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundInListExpression : BoundExpression, IInListExpression, IOperation
{
	public BoundExpression Expression { get; private set; }

	public ImmutableArray<BoundExpression> List { get; private set; }

	IOperation IInListExpression.Value => Expression;

	ImmutableArray<IOperation> IInListExpression.List => List.As<IOperation>();

	protected override OperationKind ExpressionKind => OperationKind.InListExpression;

	public BoundInListExpression(SyntaxNode syntax, BoundExpression expression, ImmutableArray<BoundExpression> list, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.InListExpression, syntax, type, hasErrors || expression.HasErrors() || list.HasErrors())
	{
		Expression = expression;
		List = list;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitInListExpression(this);
	}

	public BoundInListExpression Update(BoundExpression expression, ImmutableArray<BoundExpression> list, TypeSymbol type)
	{
		if (expression != Expression || list != List || type != Type)
		{
			return new BoundInListExpression(Syntax, expression, list, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitInListExpression(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitInListExpression(this, argument);
	}
}
