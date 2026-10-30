using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalOrderByPropertyValueSyntax : InternalPropertyValueSyntax
{
	internal readonly InternalSyntaxNode _order;

	internal override SyntaxNodeDefinition Definition => OrderByPropertyValueSyntaxDefinition.Instance.Value;

	public InternalSeparatedSyntaxList<InternalOrderByExpressionSyntax> Order => new InternalSeparatedSyntaxList<InternalOrderByExpressionSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_order));

	internal InternalOrderByPropertyValueSyntax(SyntaxKind kind, InternalSyntaxNode order, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		if (order != null)
		{
			AdjustFlagsAndWidth(order);
			_order = order;
		}
	}

	internal InternalOrderByPropertyValueSyntax(SyntaxKind kind, InternalSyntaxNode order, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		if (order != null)
		{
			AdjustFlagsAndWidth(order);
			_order = order;
		}
	}

	internal InternalOrderByPropertyValueSyntax(SyntaxKind kind, InternalSyntaxNode order)
		: base(kind)
	{
		SlotCount = 1;
		if (order != null)
		{
			AdjustFlagsAndWidth(order);
			_order = order;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _order;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new OrderByPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOrderByPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitOrderByPropertyValue(this);
	}

	public InternalOrderByPropertyValueSyntax Update(InternalSeparatedSyntaxList<InternalOrderByExpressionSyntax> order)
	{
		if (order != Order)
		{
			return InternalSyntaxFactory.OrderByPropertyValue(order).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalOrderByPropertyValueSyntax(Kind, _order, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalOrderByPropertyValueSyntax(Kind, _order, GetDiagnostics(), annotations);
	}
}
