using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class BoundPropertyValueExpression : BoundExpression
{
	protected override OperationKind ExpressionKind => OperationKind.None;

	protected BoundPropertyValueExpression(BoundKind kind, SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(kind, syntax, type, hasErrors)
	{
	}

	protected BoundPropertyValueExpression(BoundKind kind, SyntaxNode syntax, TypeSymbol type)
		: base(kind, syntax, type)
	{
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
