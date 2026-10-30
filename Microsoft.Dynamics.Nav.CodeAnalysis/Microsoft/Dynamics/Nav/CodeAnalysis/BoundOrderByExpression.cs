using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundOrderByExpression : BoundExpression
{
	public bool IsAscending { get; private set; }

	public BoundExpression Value { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.None;

	public BoundOrderByExpression(SyntaxNode syntax, bool isAscending, BoundExpression value, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.OrderByExpression, syntax, type, hasErrors || value.HasErrors())
	{
		IsAscending = isAscending;
		Value = value;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitOrderByExpression(this);
	}

	public BoundOrderByExpression Update(bool isAscending, BoundExpression value, TypeSymbol type)
	{
		if (isAscending != IsAscending || value != Value || type != Type)
		{
			return new BoundOrderByExpression(Syntax, isAscending, value, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitNoneOperation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitNoneOperation(this, argument);
	}
}
