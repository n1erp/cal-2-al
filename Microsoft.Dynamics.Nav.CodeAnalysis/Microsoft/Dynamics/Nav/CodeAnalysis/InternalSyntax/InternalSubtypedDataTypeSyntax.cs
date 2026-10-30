using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalSubtypedDataTypeSyntax : InternalDataTypeSyntax
{
	internal readonly InternalSyntaxToken _typeName;

	internal readonly InternalObjectNameOrIdSyntax _subtype;

	internal override SyntaxNodeDefinition Definition => SubtypedDataTypeSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken TypeName => _typeName;

	public InternalObjectNameOrIdSyntax Subtype => _subtype;

	internal InternalSubtypedDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalObjectNameOrIdSyntax subtype, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		AdjustFlagsAndWidth(subtype);
		_subtype = subtype;
	}

	internal InternalSubtypedDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalObjectNameOrIdSyntax subtype, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		AdjustFlagsAndWidth(subtype);
		_subtype = subtype;
	}

	internal InternalSubtypedDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalObjectNameOrIdSyntax subtype)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		AdjustFlagsAndWidth(subtype);
		_subtype = subtype;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_typeName, 
			1 => _subtype, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new SubtypedDataTypeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSubtypedDataType(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitSubtypedDataType(this);
	}

	public InternalSubtypedDataTypeSyntax Update(InternalSyntaxToken typeName, InternalObjectNameOrIdSyntax subtype)
	{
		if (typeName != TypeName || subtype != Subtype)
		{
			return InternalSyntaxFactory.SubtypedDataType(typeName, subtype).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalSubtypedDataTypeSyntax(Kind, _typeName, _subtype, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalSubtypedDataTypeSyntax(Kind, _typeName, _subtype, GetDiagnostics(), annotations);
	}
}
