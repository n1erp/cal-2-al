using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalDecimalPropertyValueSyntax : InternalLiteralPropertyValueSyntax
{
	internal readonly InternalDecimalSignedLiteralValueSyntax _value;

	internal override SyntaxNodeDefinition Definition => DecimalPropertyValueSyntaxDefinition.Instance.Value;

	public InternalDecimalSignedLiteralValueSyntax Value => _value;

	internal InternalDecimalPropertyValueSyntax(SyntaxKind kind, InternalDecimalSignedLiteralValueSyntax value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalDecimalPropertyValueSyntax(SyntaxKind kind, InternalDecimalSignedLiteralValueSyntax value, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalDecimalPropertyValueSyntax(SyntaxKind kind, InternalDecimalSignedLiteralValueSyntax value)
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
		return new DecimalPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDecimalPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitDecimalPropertyValue(this);
	}

	public InternalDecimalPropertyValueSyntax Update(InternalDecimalSignedLiteralValueSyntax value)
	{
		if (value != Value)
		{
			return InternalSyntaxFactory.DecimalPropertyValue(value).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalDecimalPropertyValueSyntax(Kind, _value, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalDecimalPropertyValueSyntax(Kind, _value, GetDiagnostics(), annotations);
	}
}
