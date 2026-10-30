using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalQueryDataItemLinkPropertyValueSyntax : InternalPropertyValueSyntax
{
	internal readonly InternalSyntaxNode _joins;

	internal override SyntaxNodeDefinition Definition => QueryDataItemLinkPropertyValueSyntaxDefinition.Instance.Value;

	public InternalSeparatedSyntaxList<InternalQueryDataItemLinkExpressionSyntax> Joins => new InternalSeparatedSyntaxList<InternalQueryDataItemLinkExpressionSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_joins));

	internal InternalQueryDataItemLinkPropertyValueSyntax(SyntaxKind kind, InternalSyntaxNode joins, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		if (joins != null)
		{
			AdjustFlagsAndWidth(joins);
			_joins = joins;
		}
	}

	internal InternalQueryDataItemLinkPropertyValueSyntax(SyntaxKind kind, InternalSyntaxNode joins, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		if (joins != null)
		{
			AdjustFlagsAndWidth(joins);
			_joins = joins;
		}
	}

	internal InternalQueryDataItemLinkPropertyValueSyntax(SyntaxKind kind, InternalSyntaxNode joins)
		: base(kind)
	{
		SlotCount = 1;
		if (joins != null)
		{
			AdjustFlagsAndWidth(joins);
			_joins = joins;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _joins;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new QueryDataItemLinkPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQueryDataItemLinkPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitQueryDataItemLinkPropertyValue(this);
	}

	public InternalQueryDataItemLinkPropertyValueSyntax Update(InternalSeparatedSyntaxList<InternalQueryDataItemLinkExpressionSyntax> joins)
	{
		if (joins != Joins)
		{
			return InternalSyntaxFactory.QueryDataItemLinkPropertyValue(joins).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalQueryDataItemLinkPropertyValueSyntax(Kind, _joins, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalQueryDataItemLinkPropertyValueSyntax(Kind, _joins, GetDiagnostics(), annotations);
	}
}
