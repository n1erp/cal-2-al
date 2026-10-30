using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalInt64PropertyValueSyntax : InternalLiteralPropertyValueSyntax
{
	internal readonly InternalInt64SignedLiteralValueSyntax _value;

	internal override SyntaxNodeDefinition Definition => Int64PropertyValueSyntaxDefinition.Instance.Value;

	public InternalInt64SignedLiteralValueSyntax Value => _value;

	internal InternalInt64PropertyValueSyntax(SyntaxKind kind, InternalInt64SignedLiteralValueSyntax value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalInt64PropertyValueSyntax(SyntaxKind kind, InternalInt64SignedLiteralValueSyntax value, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalInt64PropertyValueSyntax(SyntaxKind kind, InternalInt64SignedLiteralValueSyntax value)
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
		return new Int64PropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInt64PropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitInt64PropertyValue(this);
	}

	public InternalInt64PropertyValueSyntax Update(InternalInt64SignedLiteralValueSyntax value)
	{
		if (value != Value)
		{
			return InternalSyntaxFactory.Int64PropertyValue(value).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalInt64PropertyValueSyntax(Kind, _value, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalInt64PropertyValueSyntax(Kind, _value, GetDiagnostics(), annotations);
	}
}
