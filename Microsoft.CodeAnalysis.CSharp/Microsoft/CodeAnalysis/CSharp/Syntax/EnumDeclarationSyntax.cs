using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class EnumDeclarationSyntax : BaseTypeDeclarationSyntax
{
	private SyntaxNode attributeLists;

	private BaseListSyntax baseList;

	private SyntaxNode members;

	public override SyntaxList<AttributeListSyntax> AttributeLists => new SyntaxList<AttributeListSyntax>(GetRed(ref attributeLists, 0));

	public override SyntaxTokenList Modifiers
	{
		get
		{
			GreenNode slot = Green.GetSlot(1);
			if (slot != null)
			{
				return new SyntaxTokenList(this, slot, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken EnumKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EnumDeclarationSyntax)Green).enumKeyword, GetChildPosition(2), GetChildIndex(2));

	public override SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EnumDeclarationSyntax)Green).identifier, GetChildPosition(3), GetChildIndex(3));

	public override BaseListSyntax BaseList => GetRed(ref baseList, 4);

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EnumDeclarationSyntax)Green).openBraceToken, GetChildPosition(5), GetChildIndex(5));

	public SeparatedSyntaxList<EnumMemberDeclarationSyntax> Members
	{
		get
		{
			SyntaxNode red = GetRed(ref members, 6);
			if (red != null)
			{
				return new SeparatedSyntaxList<EnumMemberDeclarationSyntax>(red, GetChildIndex(6));
			}
			return default;
		}
	}

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EnumDeclarationSyntax)Green).closeBraceToken, GetChildPosition(7), GetChildIndex(7));

	public override SyntaxToken SemicolonToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken semicolonToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EnumDeclarationSyntax)Green).semicolonToken;
			if (semicolonToken != null)
			{
				return new SyntaxToken(this, semicolonToken, GetChildPosition(8), GetChildIndex(8));
			}
			return default;
		}
	}

	internal EnumDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			4 => GetRed(ref baseList, 4), 
			6 => GetRed(ref members, 6), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			4 => baseList, 
			6 => members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEnumDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitEnumDeclaration(this);
	}

	public EnumDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken enumKeyword, SyntaxToken identifier, BaseListSyntax baseList, SyntaxToken openBraceToken, SeparatedSyntaxList<EnumMemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || enumKeyword != EnumKeyword || identifier != Identifier || baseList != BaseList || openBraceToken != OpenBraceToken || members != Members || closeBraceToken != CloseBraceToken || semicolonToken != SemicolonToken)
		{
			EnumDeclarationSyntax enumDeclarationSyntax = SyntaxFactory.EnumDeclaration(attributeLists, modifiers, enumKeyword, identifier, baseList, openBraceToken, members, closeBraceToken, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return enumDeclarationSyntax.WithAnnotations(annotations);
			}
			return enumDeclarationSyntax;
		}
		return this;
	}

	internal override BaseTypeDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeLists(attributeLists);
	}

	public new EnumDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, EnumKeyword, Identifier, BaseList, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override BaseTypeDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers)
	{
		return WithModifiers(modifiers);
	}

	public new EnumDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, EnumKeyword, Identifier, BaseList, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	public EnumDeclarationSyntax WithEnumKeyword(SyntaxToken enumKeyword)
	{
		return Update(AttributeLists, Modifiers, enumKeyword, Identifier, BaseList, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override BaseTypeDeclarationSyntax WithIdentifierCore(SyntaxToken identifier)
	{
		return WithIdentifier(identifier);
	}

	public new EnumDeclarationSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(AttributeLists, Modifiers, EnumKeyword, identifier, BaseList, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override BaseTypeDeclarationSyntax WithBaseListCore(BaseListSyntax baseList)
	{
		return WithBaseList(baseList);
	}

	public new EnumDeclarationSyntax WithBaseList(BaseListSyntax baseList)
	{
		return Update(AttributeLists, Modifiers, EnumKeyword, Identifier, baseList, OpenBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	internal override BaseTypeDeclarationSyntax WithOpenBraceTokenCore(SyntaxToken openBraceToken)
	{
		return WithOpenBraceToken(openBraceToken);
	}

	public new EnumDeclarationSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(AttributeLists, Modifiers, EnumKeyword, Identifier, BaseList, openBraceToken, Members, CloseBraceToken, SemicolonToken);
	}

	public EnumDeclarationSyntax WithMembers(SeparatedSyntaxList<EnumMemberDeclarationSyntax> members)
	{
		return Update(AttributeLists, Modifiers, EnumKeyword, Identifier, BaseList, OpenBraceToken, members, CloseBraceToken, SemicolonToken);
	}

	internal override BaseTypeDeclarationSyntax WithCloseBraceTokenCore(SyntaxToken closeBraceToken)
	{
		return WithCloseBraceToken(closeBraceToken);
	}

	public new EnumDeclarationSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(AttributeLists, Modifiers, EnumKeyword, Identifier, BaseList, OpenBraceToken, Members, closeBraceToken, SemicolonToken);
	}

	internal override BaseTypeDeclarationSyntax WithSemicolonTokenCore(SyntaxToken semicolonToken)
	{
		return WithSemicolonToken(semicolonToken);
	}

	public new EnumDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(AttributeLists, Modifiers, EnumKeyword, Identifier, BaseList, OpenBraceToken, Members, CloseBraceToken, semicolonToken);
	}

	internal override BaseTypeDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items)
	{
		return AddAttributeLists(items);
	}

	public new EnumDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	internal override BaseTypeDeclarationSyntax AddModifiersCore(params SyntaxToken[] items)
	{
		return AddModifiers(items);
	}

	public new EnumDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	internal override BaseTypeDeclarationSyntax AddBaseListTypesCore(params BaseTypeSyntax[] items)
	{
		return AddBaseListTypes(items);
	}

	public new EnumDeclarationSyntax AddBaseListTypes(params BaseTypeSyntax[] items)
	{
		BaseListSyntax baseListSyntax = BaseList ?? SyntaxFactory.BaseList();
		return WithBaseList(baseListSyntax.WithTypes(baseListSyntax.Types.AddRange(items)));
	}

	public EnumDeclarationSyntax AddMembers(params EnumMemberDeclarationSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
