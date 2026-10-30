using System;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class PropertyDeclarationSyntax : BasePropertyDeclarationSyntax
{
	private SyntaxNode attributeLists;

	private TypeSyntax type;

	private ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier;

	private AccessorListSyntax accessorList;

	private ArrowExpressionClauseSyntax expressionBody;

	private EqualsValueClauseSyntax initializer;

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

	public override TypeSyntax Type => GetRed(ref type, 2);

	public override ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier => GetRed(ref explicitInterfaceSpecifier, 3);

	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PropertyDeclarationSyntax)Green).identifier, GetChildPosition(4), GetChildIndex(4));

	public override AccessorListSyntax AccessorList => GetRed(ref accessorList, 5);

	public ArrowExpressionClauseSyntax ExpressionBody => GetRed(ref expressionBody, 6);

	public EqualsValueClauseSyntax Initializer => GetRed(ref initializer, 7);

	public SyntaxToken SemicolonToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken semicolonToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PropertyDeclarationSyntax)Green).semicolonToken;
			if (semicolonToken != null)
			{
				return new SyntaxToken(this, semicolonToken, GetChildPosition(8), GetChildIndex(8));
			}
			return default;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This member is obsolete.", true)]
	public SyntaxToken Semicolon => SemicolonToken;

	internal PropertyDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			2 => GetRed(ref type, 2), 
			3 => GetRed(ref explicitInterfaceSpecifier, 3), 
			5 => GetRed(ref accessorList, 5), 
			6 => GetRed(ref expressionBody, 6), 
			7 => GetRed(ref initializer, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			2 => type, 
			3 => explicitInterfaceSpecifier, 
			5 => accessorList, 
			6 => expressionBody, 
			7 => initializer, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPropertyDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitPropertyDeclaration(this);
	}

	public PropertyDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, AccessorListSyntax accessorList, ArrowExpressionClauseSyntax expressionBody, EqualsValueClauseSyntax initializer, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || type != Type || explicitInterfaceSpecifier != ExplicitInterfaceSpecifier || identifier != Identifier || accessorList != AccessorList || expressionBody != ExpressionBody || initializer != Initializer || semicolonToken != SemicolonToken)
		{
			PropertyDeclarationSyntax propertyDeclarationSyntax = SyntaxFactory.PropertyDeclaration(attributeLists, modifiers, type, explicitInterfaceSpecifier, identifier, accessorList, expressionBody, initializer, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return propertyDeclarationSyntax.WithAnnotations(annotations);
			}
			return propertyDeclarationSyntax;
		}
		return this;
	}

	internal override BasePropertyDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeLists(attributeLists);
	}

	public new PropertyDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, Type, ExplicitInterfaceSpecifier, Identifier, AccessorList, ExpressionBody, Initializer, SemicolonToken);
	}

	internal override BasePropertyDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers)
	{
		return WithModifiers(modifiers);
	}

	public new PropertyDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, Type, ExplicitInterfaceSpecifier, Identifier, AccessorList, ExpressionBody, Initializer, SemicolonToken);
	}

	internal override BasePropertyDeclarationSyntax WithTypeCore(TypeSyntax type)
	{
		return WithType(type);
	}

	public new PropertyDeclarationSyntax WithType(TypeSyntax type)
	{
		return Update(AttributeLists, Modifiers, type, ExplicitInterfaceSpecifier, Identifier, AccessorList, ExpressionBody, Initializer, SemicolonToken);
	}

	internal override BasePropertyDeclarationSyntax WithExplicitInterfaceSpecifierCore(ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier)
	{
		return WithExplicitInterfaceSpecifier(explicitInterfaceSpecifier);
	}

	public new PropertyDeclarationSyntax WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier)
	{
		return Update(AttributeLists, Modifiers, Type, explicitInterfaceSpecifier, Identifier, AccessorList, ExpressionBody, Initializer, SemicolonToken);
	}

	public PropertyDeclarationSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(AttributeLists, Modifiers, Type, ExplicitInterfaceSpecifier, identifier, AccessorList, ExpressionBody, Initializer, SemicolonToken);
	}

	internal override BasePropertyDeclarationSyntax WithAccessorListCore(AccessorListSyntax accessorList)
	{
		return WithAccessorList(accessorList);
	}

	public new PropertyDeclarationSyntax WithAccessorList(AccessorListSyntax accessorList)
	{
		return Update(AttributeLists, Modifiers, Type, ExplicitInterfaceSpecifier, Identifier, accessorList, ExpressionBody, Initializer, SemicolonToken);
	}

	public PropertyDeclarationSyntax WithExpressionBody(ArrowExpressionClauseSyntax expressionBody)
	{
		return Update(AttributeLists, Modifiers, Type, ExplicitInterfaceSpecifier, Identifier, AccessorList, expressionBody, Initializer, SemicolonToken);
	}

	public PropertyDeclarationSyntax WithInitializer(EqualsValueClauseSyntax initializer)
	{
		return Update(AttributeLists, Modifiers, Type, ExplicitInterfaceSpecifier, Identifier, AccessorList, ExpressionBody, initializer, SemicolonToken);
	}

	public PropertyDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(AttributeLists, Modifiers, Type, ExplicitInterfaceSpecifier, Identifier, AccessorList, ExpressionBody, Initializer, semicolonToken);
	}

	internal override BasePropertyDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items)
	{
		return AddAttributeLists(items);
	}

	public new PropertyDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	internal override BasePropertyDeclarationSyntax AddModifiersCore(params SyntaxToken[] items)
	{
		return AddModifiers(items);
	}

	public new PropertyDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	internal override BasePropertyDeclarationSyntax AddAccessorListAccessorsCore(params AccessorDeclarationSyntax[] items)
	{
		return AddAccessorListAccessors(items);
	}

	public new PropertyDeclarationSyntax AddAccessorListAccessors(params AccessorDeclarationSyntax[] items)
	{
		AccessorListSyntax accessorListSyntax = AccessorList ?? SyntaxFactory.AccessorList();
		return WithAccessorList(accessorListSyntax.WithAccessors(accessorListSyntax.Accessors.AddRange(items)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This member is obsolete.", true)]
	public PropertyDeclarationSyntax WithSemicolon(SyntaxToken semicolon)
	{
		return WithSemicolonToken(semicolon);
	}
}
