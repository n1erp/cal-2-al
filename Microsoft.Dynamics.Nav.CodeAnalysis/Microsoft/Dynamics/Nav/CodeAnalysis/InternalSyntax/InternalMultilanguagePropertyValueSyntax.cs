using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalMultilanguagePropertyValueSyntax : InternalPropertyValueSyntax
{
	internal readonly InternalCommaSeparatedIdentifierEqualsStringListSyntax _value;

	internal override SyntaxNodeDefinition Definition => MultilanguagePropertyValueSyntaxDefinition.Instance.Value;

	public InternalCommaSeparatedIdentifierEqualsStringListSyntax Value => _value;

	internal InternalMultilanguagePropertyValueSyntax(SyntaxKind kind, InternalCommaSeparatedIdentifierEqualsStringListSyntax value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalMultilanguagePropertyValueSyntax(SyntaxKind kind, InternalCommaSeparatedIdentifierEqualsStringListSyntax value, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalMultilanguagePropertyValueSyntax(SyntaxKind kind, InternalCommaSeparatedIdentifierEqualsStringListSyntax value)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _value;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new MultilanguagePropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMultilanguagePropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitMultilanguagePropertyValue(this);
	}

	public InternalMultilanguagePropertyValueSyntax Update(InternalCommaSeparatedIdentifierEqualsStringListSyntax value)
	{
		if (value != Value)
		{
			return InternalSyntaxFactory.MultilanguagePropertyValue(value).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalMultilanguagePropertyValueSyntax(Kind, _value, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalMultilanguagePropertyValueSyntax(Kind, _value, GetDiagnostics(), annotations);
	}
}
