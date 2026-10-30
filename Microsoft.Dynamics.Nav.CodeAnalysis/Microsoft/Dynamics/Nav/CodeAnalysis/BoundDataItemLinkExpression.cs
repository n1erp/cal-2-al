using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundDataItemLinkExpression : BoundExpression
{
	public BoundFieldAccess SourceField { get; private set; }

	public BoundFieldAccess RelatedField { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.None;

	public BoundDataItemLinkExpression(SyntaxNode syntax, BoundFieldAccess sourceField, BoundFieldAccess relatedField, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.DataItemLinkExpression, syntax, type, hasErrors || sourceField.HasErrors() || relatedField.HasErrors())
	{
		SourceField = sourceField;
		RelatedField = relatedField;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDataItemLinkExpression(this);
	}

	public BoundDataItemLinkExpression Update(BoundFieldAccess sourceField, BoundFieldAccess relatedField, TypeSymbol type)
	{
		if (sourceField != SourceField || relatedField != RelatedField || type != Type)
		{
			return new BoundDataItemLinkExpression(Syntax, sourceField, relatedField, type, HasErrors)
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
