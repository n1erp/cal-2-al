using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalPageFieldReferencePropertyValueSyntax : InternalIdentifierNamePropertyValueSyntax
{
	internal readonly InternalIdentifierNameSyntax _value;

	internal override SyntaxNodeDefinition Definition => PageFieldReferencePropertyValueSyntaxDefinition.Instance.Value;

	public override InternalIdentifierNameSyntax Value => _value;

	internal InternalPageFieldReferencePropertyValueSyntax(SyntaxKind kind, InternalIdentifierNameSyntax value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalPageFieldReferencePropertyValueSyntax(SyntaxKind kind, InternalIdentifierNameSyntax value, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalPageFieldReferencePropertyValueSyntax(SyntaxKind kind, InternalIdentifierNameSyntax value)
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
		return new PageFieldReferencePropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageFieldReferencePropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitPageFieldReferencePropertyValue(this);
	}

	public InternalPageFieldReferencePropertyValueSyntax Update(InternalIdentifierNameSyntax value)
	{
		if (value != Value)
		{
			return InternalSyntaxFactory.PageFieldReferencePropertyValue(value).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalPageFieldReferencePropertyValueSyntax(Kind, _value, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalPageFieldReferencePropertyValueSyntax(Kind, _value, GetDiagnostics(), annotations);
	}
}
