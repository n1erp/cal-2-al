using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalLabelPropertyValueSyntax : InternalPropertyValueSyntax
{
	internal readonly InternalLabelSyntax _value;

	internal override SyntaxNodeDefinition Definition => LabelPropertyValueSyntaxDefinition.Instance.Value;

	public InternalLabelSyntax Value => _value;

	internal InternalLabelPropertyValueSyntax(SyntaxKind kind, InternalLabelSyntax value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalLabelPropertyValueSyntax(SyntaxKind kind, InternalLabelSyntax value, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalLabelPropertyValueSyntax(SyntaxKind kind, InternalLabelSyntax value)
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
		return new LabelPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLabelPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitLabelPropertyValue(this);
	}

	public InternalLabelPropertyValueSyntax Update(InternalLabelSyntax value)
	{
		if (value != Value)
		{
			return InternalSyntaxFactory.LabelPropertyValue(value).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalLabelPropertyValueSyntax(Kind, _value, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalLabelPropertyValueSyntax(Kind, _value, GetDiagnostics(), annotations);
	}
}
