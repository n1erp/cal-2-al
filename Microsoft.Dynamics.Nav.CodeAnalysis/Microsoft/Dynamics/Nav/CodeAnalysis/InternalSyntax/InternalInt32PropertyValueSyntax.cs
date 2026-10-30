using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalInt32PropertyValueSyntax : InternalLiteralPropertyValueSyntax
{
	internal readonly InternalInt32SignedLiteralValueSyntax _value;

	internal override SyntaxNodeDefinition Definition => Int32PropertyValueSyntaxDefinition.Instance.Value;

	public InternalInt32SignedLiteralValueSyntax Value => _value;

	internal InternalInt32PropertyValueSyntax(SyntaxKind kind, InternalInt32SignedLiteralValueSyntax value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalInt32PropertyValueSyntax(SyntaxKind kind, InternalInt32SignedLiteralValueSyntax value, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalInt32PropertyValueSyntax(SyntaxKind kind, InternalInt32SignedLiteralValueSyntax value)
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
		return new Int32PropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInt32PropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitInt32PropertyValue(this);
	}

	public InternalInt32PropertyValueSyntax Update(InternalInt32SignedLiteralValueSyntax value)
	{
		if (value != Value)
		{
			return InternalSyntaxFactory.Int32PropertyValue(value).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalInt32PropertyValueSyntax(Kind, _value, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalInt32PropertyValueSyntax(Kind, _value, GetDiagnostics(), annotations);
	}
}
