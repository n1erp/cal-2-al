using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class BasePropertyDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract SyntaxList<AttributeListSyntax> AttributeLists { get; }

	public abstract SyntaxTokenList Modifiers { get; }

	public abstract TypeSyntax Type { get; }

	public abstract ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier { get; }

	public abstract AccessorListSyntax AccessorList { get; }

	internal BasePropertyDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public BasePropertyDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeListsCore(attributeLists);
	}

	internal abstract BasePropertyDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists);

	public BasePropertyDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return AddAttributeListsCore(items);
	}

	internal abstract BasePropertyDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items);

	public BasePropertyDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return WithModifiersCore(modifiers);
	}

	internal abstract BasePropertyDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers);

	public BasePropertyDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return AddModifiersCore(items);
	}

	internal abstract BasePropertyDeclarationSyntax AddModifiersCore(params SyntaxToken[] items);

	public BasePropertyDeclarationSyntax WithType(TypeSyntax type)
	{
		return WithTypeCore(type);
	}

	internal abstract BasePropertyDeclarationSyntax WithTypeCore(TypeSyntax type);

	public BasePropertyDeclarationSyntax WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier)
	{
		return WithExplicitInterfaceSpecifierCore(explicitInterfaceSpecifier);
	}

	internal abstract BasePropertyDeclarationSyntax WithExplicitInterfaceSpecifierCore(ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier);

	public BasePropertyDeclarationSyntax WithAccessorList(AccessorListSyntax accessorList)
	{
		return WithAccessorListCore(accessorList);
	}

	internal abstract BasePropertyDeclarationSyntax WithAccessorListCore(AccessorListSyntax accessorList);

	public BasePropertyDeclarationSyntax AddAccessorListAccessors(params AccessorDeclarationSyntax[] items)
	{
		return AddAccessorListAccessorsCore(items);
	}

	internal abstract BasePropertyDeclarationSyntax AddAccessorListAccessorsCore(params AccessorDeclarationSyntax[] items);
}
