using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class EnumValueSyntax : SyntaxNode
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	public SyntaxToken EnumValueToken => new SyntaxToken(this, ((InternalEnumValueSyntax)Node)._enumValueToken, Position, 0);

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalEnumValueSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken Id => new SyntaxToken(this, ((InternalEnumValueSyntax)Node)._id, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalEnumValueSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	public IdentifierNameSyntax Name => GetRed(ref _name, 4);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalEnumValueSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalEnumValueSyntax)Node)._openBraceToken, GetChildPosition(6), GetChildIndex(6));

	public PropertyListSyntax PropertyList => GetRed(ref _propertyList, 7);

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalEnumValueSyntax)Node)._closeBraceToken, GetChildPosition(8), GetChildIndex(8));

	internal EnumValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			4 => (SyntaxNode)GetRed(ref _name, 4), 
			7 => GetRed(ref _propertyList, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			4 => (SyntaxNode)_name, 
			7 => _propertyList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEnumValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitEnumValue(this);
	}

	public EnumValueSyntax Update(SyntaxToken enumValueToken, SyntaxToken openParenthesisToken, SyntaxToken id, SyntaxToken semicolonToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		if (enumValueToken != EnumValueToken || openParenthesisToken != OpenParenthesisToken || id != Id || semicolonToken != SemicolonToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			EnumValueSyntax enumValueSyntax = SyntaxFactory.EnumValue(enumValueToken, openParenthesisToken, id, semicolonToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return enumValueSyntax.WithAnnotations(annotations);
			}
			return enumValueSyntax;
		}
		return this;
	}

	public EnumValueSyntax WithEnumValueToken(SyntaxToken enumValueToken)
	{
		return Update(enumValueToken, OpenParenthesisToken, Id, SemicolonToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public EnumValueSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(EnumValueToken, openParenthesisToken, Id, SemicolonToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public EnumValueSyntax WithId(SyntaxToken id)
	{
		return Update(EnumValueToken, OpenParenthesisToken, id, SemicolonToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public EnumValueSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(EnumValueToken, OpenParenthesisToken, Id, semicolonToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public EnumValueSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(EnumValueToken, OpenParenthesisToken, Id, SemicolonToken, name, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public EnumValueSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(EnumValueToken, OpenParenthesisToken, Id, SemicolonToken, Name, closeParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public EnumValueSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(EnumValueToken, OpenParenthesisToken, Id, SemicolonToken, Name, CloseParenthesisToken, openBraceToken, PropertyList, CloseBraceToken);
	}

	public EnumValueSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(EnumValueToken, OpenParenthesisToken, Id, SemicolonToken, Name, CloseParenthesisToken, OpenBraceToken, propertyList, CloseBraceToken);
	}

	public EnumValueSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(EnumValueToken, OpenParenthesisToken, Id, SemicolonToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, closeBraceToken);
	}

	public EnumValueSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		PropertyListSyntax propertyListSyntax = PropertyList ?? SyntaxFactory.PropertyList();
		return WithPropertyList(propertyListSyntax.WithProperties(propertyListSyntax.Properties.AddRange(items)));
	}
}
