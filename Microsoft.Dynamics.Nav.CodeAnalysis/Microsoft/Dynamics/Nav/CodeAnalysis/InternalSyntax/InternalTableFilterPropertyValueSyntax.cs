using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalTableFilterPropertyValueSyntax : InternalPropertyValueSyntax
{
	internal readonly InternalSyntaxNode _conditions;

	internal override SyntaxNodeDefinition Definition => TableFilterPropertyValueSyntaxDefinition.Instance.Value;

	public InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> Conditions => new InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_conditions));

	internal InternalTableFilterPropertyValueSyntax(SyntaxKind kind, InternalSyntaxNode conditions, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		if (conditions != null)
		{
			AdjustFlagsAndWidth(conditions);
			_conditions = conditions;
		}
	}

	internal InternalTableFilterPropertyValueSyntax(SyntaxKind kind, InternalSyntaxNode conditions, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		if (conditions != null)
		{
			AdjustFlagsAndWidth(conditions);
			_conditions = conditions;
		}
	}

	internal InternalTableFilterPropertyValueSyntax(SyntaxKind kind, InternalSyntaxNode conditions)
		: base(kind)
	{
		SlotCount = 1;
		if (conditions != null)
		{
			AdjustFlagsAndWidth(conditions);
			_conditions = conditions;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _conditions;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new TableFilterPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTableFilterPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitTableFilterPropertyValue(this);
	}

	public InternalTableFilterPropertyValueSyntax Update(InternalSeparatedSyntaxList<InternalPropertyExpressionSyntax> conditions)
	{
		if (conditions != Conditions)
		{
			return InternalSyntaxFactory.TableFilterPropertyValue(conditions).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalTableFilterPropertyValueSyntax(Kind, _conditions, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalTableFilterPropertyValueSyntax(Kind, _conditions, GetDiagnostics(), annotations);
	}
}
