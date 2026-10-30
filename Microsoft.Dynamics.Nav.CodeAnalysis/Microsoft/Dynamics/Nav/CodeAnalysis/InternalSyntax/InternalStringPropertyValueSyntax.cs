using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalStringPropertyValueSyntax : InternalLiteralPropertyValueSyntax
{
	internal readonly InternalStringLiteralValueSyntax _value;

	internal override SyntaxNodeDefinition Definition => StringPropertyValueSyntaxDefinition.Instance.Value;

	public InternalStringLiteralValueSyntax Value => _value;

	internal InternalStringPropertyValueSyntax(SyntaxKind kind, InternalStringLiteralValueSyntax value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalStringPropertyValueSyntax(SyntaxKind kind, InternalStringLiteralValueSyntax value, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalStringPropertyValueSyntax(SyntaxKind kind, InternalStringLiteralValueSyntax value)
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
		return new StringPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitStringPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitStringPropertyValue(this);
	}

	public InternalStringPropertyValueSyntax Update(InternalStringLiteralValueSyntax value)
	{
		if (value != Value)
		{
			return InternalSyntaxFactory.StringPropertyValue(value).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalStringPropertyValueSyntax(Kind, _value, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalStringPropertyValueSyntax(Kind, _value, GetDiagnostics(), annotations);
	}
}
