using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalDecimalPlacesPropertyValueSyntax : InternalPropertyValueSyntax
{
	internal readonly InternalDecimalPlacesSyntax _value;

	internal override SyntaxNodeDefinition Definition => DecimalPlacesPropertyValueSyntaxDefinition.Instance.Value;

	public InternalDecimalPlacesSyntax Value => _value;

	internal InternalDecimalPlacesPropertyValueSyntax(SyntaxKind kind, InternalDecimalPlacesSyntax value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalDecimalPlacesPropertyValueSyntax(SyntaxKind kind, InternalDecimalPlacesSyntax value, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalDecimalPlacesPropertyValueSyntax(SyntaxKind kind, InternalDecimalPlacesSyntax value)
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
		return new DecimalPlacesPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDecimalPlacesPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitDecimalPlacesPropertyValue(this);
	}

	public InternalDecimalPlacesPropertyValueSyntax Update(InternalDecimalPlacesSyntax value)
	{
		if (value != Value)
		{
			return InternalSyntaxFactory.DecimalPlacesPropertyValue(value).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalDecimalPlacesPropertyValueSyntax(Kind, _value, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalDecimalPlacesPropertyValueSyntax(Kind, _value, GetDiagnostics(), annotations);
	}
}
