using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalEnumDataTypeSyntax : InternalDataTypeSyntax
{
	internal readonly InternalSyntaxToken _typeName;

	internal readonly InternalIdentifierNameSyntax _enumTypeName;

	internal override SyntaxNodeDefinition Definition => EnumDataTypeSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken TypeName => _typeName;

	public InternalIdentifierNameSyntax EnumTypeName => _enumTypeName;

	internal InternalEnumDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalIdentifierNameSyntax enumTypeName, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		AdjustFlagsAndWidth(enumTypeName);
		_enumTypeName = enumTypeName;
	}

	internal InternalEnumDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalIdentifierNameSyntax enumTypeName, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		AdjustFlagsAndWidth(enumTypeName);
		_enumTypeName = enumTypeName;
	}

	internal InternalEnumDataTypeSyntax(SyntaxKind kind, InternalSyntaxToken typeName, InternalIdentifierNameSyntax enumTypeName)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(typeName);
		_typeName = typeName;
		AdjustFlagsAndWidth(enumTypeName);
		_enumTypeName = enumTypeName;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_typeName, 
			1 => _enumTypeName, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new EnumDataTypeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEnumDataType(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitEnumDataType(this);
	}

	public InternalEnumDataTypeSyntax Update(InternalSyntaxToken typeName, InternalIdentifierNameSyntax enumTypeName)
	{
		if (typeName != TypeName || enumTypeName != EnumTypeName)
		{
			return InternalSyntaxFactory.EnumDataType(typeName, enumTypeName).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalEnumDataTypeSyntax(Kind, _typeName, _enumTypeName, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalEnumDataTypeSyntax(Kind, _typeName, _enumTypeName, GetDiagnostics(), annotations);
	}
}
