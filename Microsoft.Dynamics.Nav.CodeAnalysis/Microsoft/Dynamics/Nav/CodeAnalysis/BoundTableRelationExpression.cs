using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundTableRelationExpression : BoundExpression
{
	public BoundTableRelationConditionExpression IfExpression { get; private set; }

	public BoundExpression FieldAccess { get; private set; }

	public BoundTableRelationConditionExpression TableFilter { get; private set; }

	public BoundTableRelationExpression ElseExpression { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.None;

	public BoundTableRelationExpression(SyntaxNode syntax, BoundTableRelationConditionExpression ifExpression, BoundExpression fieldAccess, BoundTableRelationConditionExpression tableFilter, BoundTableRelationExpression elseExpression, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TableRelationExpression, syntax, type, hasErrors || ifExpression.HasErrors() || fieldAccess.HasErrors() || tableFilter.HasErrors() || elseExpression.HasErrors())
	{
		IfExpression = ifExpression;
		FieldAccess = fieldAccess;
		TableFilter = tableFilter;
		ElseExpression = elseExpression;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTableRelationExpression(this);
	}

	public BoundTableRelationExpression Update(BoundTableRelationConditionExpression ifExpression, BoundExpression fieldAccess, BoundTableRelationConditionExpression tableFilter, BoundTableRelationExpression elseExpression, TypeSymbol type)
	{
		if (ifExpression != IfExpression || fieldAccess != FieldAccess || tableFilter != TableFilter || elseExpression != ElseExpression || type != Type)
		{
			return new BoundTableRelationExpression(Syntax, ifExpression, fieldAccess, tableFilter, elseExpression, type, HasErrors)
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
