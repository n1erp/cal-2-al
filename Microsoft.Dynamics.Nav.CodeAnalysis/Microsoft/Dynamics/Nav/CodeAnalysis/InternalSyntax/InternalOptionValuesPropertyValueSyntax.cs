using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalOptionValuesPropertyValueSyntax : InternalLiteralPropertyValueSyntax
{
	internal readonly InternalOptionValuesSyntax _value;

	internal override SyntaxNodeDefinition Definition => OptionValuesPropertyValueSyntaxDefinition.Instance.Value;

	public InternalOptionValuesSyntax Value => _value;

	internal InternalOptionValuesPropertyValueSyntax(SyntaxKind kind, InternalOptionValuesSyntax value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalOptionValuesPropertyValueSyntax(SyntaxKind kind, InternalOptionValuesSyntax value, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalOptionValuesPropertyValueSyntax(SyntaxKind kind, InternalOptionValuesSyntax value)
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
		return new OptionValuesPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOptionValuesPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitOptionValuesPropertyValue(this);
	}

	public InternalOptionValuesPropertyValueSyntax Update(InternalOptionValuesSyntax value)
	{
		if (value != Value)
		{
			return InternalSyntaxFactory.OptionValuesPropertyValue(value).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalOptionValuesPropertyValueSyntax(Kind, _value, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalOptionValuesPropertyValueSyntax(Kind, _value, GetDiagnostics(), annotations);
	}
}
