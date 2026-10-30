using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundTableRelationConditionExpression : BoundExpression
{
	public ImmutableArray<BoundEqualsPropertyExpression> Condition { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.None;

	public BoundTableRelationConditionExpression(SyntaxNode syntax, ImmutableArray<BoundEqualsPropertyExpression> condition, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TableRelationConditionExpression, syntax, type, hasErrors || condition.HasErrors())
	{
		Condition = condition;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTableRelationConditionExpression(this);
	}

	public BoundTableRelationConditionExpression Update(ImmutableArray<BoundEqualsPropertyExpression> condition, TypeSymbol type)
	{
		if (condition != Condition || type != Type)
		{
			return new BoundTableRelationConditionExpression(Syntax, condition, type, HasErrors)
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
