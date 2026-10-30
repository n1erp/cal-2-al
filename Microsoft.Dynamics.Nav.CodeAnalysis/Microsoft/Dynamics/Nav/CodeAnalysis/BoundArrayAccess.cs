using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundArrayAccess : BoundExpression, IArrayElementReferenceExpression, IOperation
{
	public override Symbol ExpressionSymbol => Type;

	public BoundExpression Expression { get; private set; }

	public ImmutableArray<BoundExpression> Indices { get; private set; }

	IOperation IArrayElementReferenceExpression.ArrayReference => Expression;

	ImmutableArray<IOperation> IArrayElementReferenceExpression.Indexes => Indices.As<IOperation>();

	protected override OperationKind ExpressionKind => OperationKind.ArrayElementReferenceExpression;

	public BoundArrayAccess(SyntaxNode syntax, BoundExpression expression, ImmutableArray<BoundExpression> indices, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ArrayAccess, syntax, type, hasErrors || expression.HasErrors() || indices.HasErrors())
	{
		Expression = expression;
		Indices = indices;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitArrayAccess(this);
	}

	public BoundArrayAccess Update(BoundExpression expression, ImmutableArray<BoundExpression> indices, TypeSymbol type)
	{
		if (expression != Expression || indices != Indices || type != Type)
		{
			return new BoundArrayAccess(Syntax, expression, indices, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitArrayElementReferenceExpression(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitArrayElementReferenceExpression(this, argument);
	}
}
