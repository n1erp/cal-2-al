using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundEqualsPropertyExpression : BoundExpression
{
	public BoundExpression Left { get; private set; }

	public BoundPropertyRhsExpression Right { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.None;

	public BoundEqualsPropertyExpression(SyntaxNode syntax, BoundExpression left, BoundPropertyRhsExpression right, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.EqualsPropertyExpression, syntax, type, hasErrors || left.HasErrors() || right.HasErrors())
	{
		Left = left;
		Right = right;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitEqualsPropertyExpression(this);
	}

	public BoundEqualsPropertyExpression Update(BoundExpression left, BoundPropertyRhsExpression right, TypeSymbol type)
	{
		if (left != Left || right != Right || type != Type)
		{
			return new BoundEqualsPropertyExpression(Syntax, left, right, type, HasErrors)
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
