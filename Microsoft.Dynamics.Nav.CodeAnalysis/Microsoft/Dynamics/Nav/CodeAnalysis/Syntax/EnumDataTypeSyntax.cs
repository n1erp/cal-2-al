using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class EnumDataTypeSyntax : DataTypeSyntax
{
	private IdentifierNameSyntax _enumTypeName;

	public override SyntaxToken TypeName => new SyntaxToken(this, ((InternalEnumDataTypeSyntax)Node)._typeName, Position, 0);

	public IdentifierNameSyntax EnumTypeName => GetRed(ref _enumTypeName, 1);

	internal EnumDataTypeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _enumTypeName, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _enumTypeName;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEnumDataType(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitEnumDataType(this);
	}

	public EnumDataTypeSyntax Update(SyntaxToken typeName, IdentifierNameSyntax enumTypeName)
	{
		if (typeName != TypeName || enumTypeName != EnumTypeName)
		{
			EnumDataTypeSyntax enumDataTypeSyntax = SyntaxFactory.EnumDataType(typeName, enumTypeName);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return enumDataTypeSyntax.WithAnnotations(annotations);
			}
			return enumDataTypeSyntax;
		}
		return this;
	}

	public EnumDataTypeSyntax WithTypeName(SyntaxToken typeName)
	{
		return Update(typeName, EnumTypeName);
	}

	public EnumDataTypeSyntax WithEnumTypeName(IdentifierNameSyntax enumTypeName)
	{
		return Update(TypeName, enumTypeName);
	}
}
