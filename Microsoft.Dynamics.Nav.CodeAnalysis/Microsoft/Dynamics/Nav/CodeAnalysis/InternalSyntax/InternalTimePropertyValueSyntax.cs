using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalTimePropertyValueSyntax : InternalLiteralPropertyValueSyntax
{
	internal readonly InternalTimeLiteralValueSyntax _value;

	internal override SyntaxNodeDefinition Definition => TimePropertyValueSyntaxDefinition.Instance.Value;

	public InternalTimeLiteralValueSyntax Value => _value;

	internal InternalTimePropertyValueSyntax(SyntaxKind kind, InternalTimeLiteralValueSyntax value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalTimePropertyValueSyntax(SyntaxKind kind, InternalTimeLiteralValueSyntax value, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalTimePropertyValueSyntax(SyntaxKind kind, InternalTimeLiteralValueSyntax value)
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
		return new TimePropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTimePropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitTimePropertyValue(this);
	}

	public InternalTimePropertyValueSyntax Update(InternalTimeLiteralValueSyntax value)
	{
		if (value != Value)
		{
			return InternalSyntaxFactory.TimePropertyValue(value).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalTimePropertyValueSyntax(Kind, _value, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalTimePropertyValueSyntax(Kind, _value, GetDiagnostics(), annotations);
	}
}
