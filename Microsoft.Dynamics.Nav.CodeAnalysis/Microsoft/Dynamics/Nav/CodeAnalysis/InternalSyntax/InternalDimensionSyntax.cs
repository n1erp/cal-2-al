using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalDimensionSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _value;

	internal override SyntaxNodeDefinition Definition => DimensionSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken Value => _value;

	internal InternalDimensionSyntax(SyntaxKind kind, InternalSyntaxToken value, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalDimensionSyntax(SyntaxKind kind, InternalSyntaxToken value, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(value);
		_value = value;
	}

	internal InternalDimensionSyntax(SyntaxKind kind, InternalSyntaxToken value)
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
		return new DimensionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDimension(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitDimension(this);
	}

	public InternalDimensionSyntax Update(InternalSyntaxToken value)
	{
		if (value != Value)
		{
			return InternalSyntaxFactory.Dimension(value).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalDimensionSyntax(Kind, _value, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalDimensionSyntax(Kind, _value, GetDiagnostics(), annotations);
	}
}
