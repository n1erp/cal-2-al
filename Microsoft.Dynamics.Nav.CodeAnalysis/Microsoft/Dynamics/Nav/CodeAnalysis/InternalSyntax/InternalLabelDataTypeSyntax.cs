using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalLabelDataTypeSyntax : InternalDataTypeSyntax
{
	internal readonly InternalSyntaxToken _typeName;

	internal readonly InternalLabelSyntax _label;

	internal override SyntaxNodeDefinition Definition => LabelDataTypeSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken TypeName => _typeName;

	public InternalLabelSyntax Label => _label;

	internal InternalLabelDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalLabelSyntax label, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		AdjustFlagsAndWidth(label);
		_label = label;
	}

	internal InternalLabelDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalLabelSyntax label, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		AdjustFlagsAndWidth(label);
		_label = label;
	}

	internal InternalLabelDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalLabelSyntax label)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		AdjustFlagsAndWidth(label);
		_label = label;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_typeName, 
			1 => _label, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new LabelDataTypeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLabelDataType(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitLabelDataType(this);
	}

	public InternalLabelDataTypeSyntax Update(InternalSyntaxToken typeName, InternalLabelSyntax label)
	{
		if (typeName != TypeName || label != Label)
		{
			return InternalSyntaxFactory.LabelDataType(typeName, label).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalLabelDataTypeSyntax(Kind, _typeName, _label, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalLabelDataTypeSyntax(Kind, _typeName, _label, GetDiagnostics(), annotations);
	}
}
