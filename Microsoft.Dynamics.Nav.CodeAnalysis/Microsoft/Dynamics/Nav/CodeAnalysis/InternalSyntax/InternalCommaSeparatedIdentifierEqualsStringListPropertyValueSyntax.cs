using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax : InternalSeparatedListPropertyValueSyntax
{
	internal readonly InternalCommaSeparatedIdentifierEqualsStringListSyntax _list;

	internal override SyntaxNodeDefinition Definition => CommaSeparatedIdentifierEqualsStringListPropertyValueSyntaxDefinition.Instance.Value;

	public InternalCommaSeparatedIdentifierEqualsStringListSyntax List => _list;

	internal InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax(SyntaxKind kind, InternalCommaSeparatedIdentifierEqualsStringListSyntax list, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(list);
		_list = list;
	}

	internal InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax(SyntaxKind kind, InternalCommaSeparatedIdentifierEqualsStringListSyntax list, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(list);
		_list = list;
	}

	internal InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax(SyntaxKind kind, InternalCommaSeparatedIdentifierEqualsStringListSyntax list)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(list);
		_list = list;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _list;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCommaSeparatedIdentifierEqualsStringListPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitCommaSeparatedIdentifierEqualsStringListPropertyValue(this);
	}

	public InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax Update(InternalCommaSeparatedIdentifierEqualsStringListSyntax list)
	{
		if (list != List)
		{
			return InternalSyntaxFactory.CommaSeparatedIdentifierEqualsStringListPropertyValue(list).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax(Kind, _list, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax(Kind, _list, GetDiagnostics(), annotations);
	}
}
