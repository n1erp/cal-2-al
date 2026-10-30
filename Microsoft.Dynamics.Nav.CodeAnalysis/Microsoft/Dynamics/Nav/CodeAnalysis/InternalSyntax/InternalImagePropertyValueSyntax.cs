using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalImagePropertyValueSyntax : InternalIdentifierNamePropertyValueSyntax
{
	internal readonly InternalIdentifierNameSyntax _value;

	internal override SyntaxNodeDefinition Definition => ImagePropertyValueSyntaxDefinition.Instance.Value;

	public override InternalIdentifierNameSyntax Value => _value;

	internal InternalImagePropertyValueSyntax(SyntaxKind kind, InternalIdentifierNameSyntax value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalImagePropertyValueSyntax(SyntaxKind kind, InternalIdentifierNameSyntax value, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalImagePropertyValueSyntax(SyntaxKind kind, InternalIdentifierNameSyntax value)
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
		return new ImagePropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitImagePropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitImagePropertyValue(this);
	}

	public InternalImagePropertyValueSyntax Update(InternalIdentifierNameSyntax value)
	{
		if (value != Value)
		{
			return InternalSyntaxFactory.ImagePropertyValue(value).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalImagePropertyValueSyntax(Kind, _value, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalImagePropertyValueSyntax(Kind, _value, GetDiagnostics(), annotations);
	}
}
