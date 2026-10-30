using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalDateTimePropertyValueSyntax : InternalLiteralPropertyValueSyntax
{
	internal readonly InternalDateTimeLiteralValueSyntax _value;

	internal override SyntaxNodeDefinition Definition => DateTimePropertyValueSyntaxDefinition.Instance.Value;

	public InternalDateTimeLiteralValueSyntax Value => _value;

	internal InternalDateTimePropertyValueSyntax(SyntaxKind kind, InternalDateTimeLiteralValueSyntax value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalDateTimePropertyValueSyntax(SyntaxKind kind, InternalDateTimeLiteralValueSyntax value, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalDateTimePropertyValueSyntax(SyntaxKind kind, InternalDateTimeLiteralValueSyntax value)
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
		return new DateTimePropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDateTimePropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitDateTimePropertyValue(this);
	}

	public InternalDateTimePropertyValueSyntax Update(InternalDateTimeLiteralValueSyntax value)
	{
		if (value != Value)
		{
			return InternalSyntaxFactory.DateTimePropertyValue(value).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalDateTimePropertyValueSyntax(Kind, _value, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalDateTimePropertyValueSyntax(Kind, _value, GetDiagnostics(), annotations);
	}
}
