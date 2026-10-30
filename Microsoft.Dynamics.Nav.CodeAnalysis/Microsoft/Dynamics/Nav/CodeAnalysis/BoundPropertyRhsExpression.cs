using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundPropertyRhsExpression : BoundExpression
{
	public SyntaxKind Operator { get; private set; }

	public BoundPropertyValueExpression Value { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.None;

	public BoundPropertyRhsExpression(SyntaxNode syntax, SyntaxKind @operator, BoundPropertyValueExpression value, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.PropertyRhsExpression, syntax, type, hasErrors || value.HasErrors())
	{
		Operator = @operator;
		Value = value;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPropertyRhsExpression(this);
	}

	public BoundPropertyRhsExpression Update(SyntaxKind @operator, BoundPropertyValueExpression value, TypeSymbol type)
	{
		if (@operator != Operator || value != Value || type != Type)
		{
			return new BoundPropertyRhsExpression(Syntax, @operator, value, type, HasErrors)
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
