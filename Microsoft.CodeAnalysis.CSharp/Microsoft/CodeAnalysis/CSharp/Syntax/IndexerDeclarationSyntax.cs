using System;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class IndexerDeclarationSyntax : BasePropertyDeclarationSyntax
{
	private SyntaxNode attributeLists;

	private TypeSyntax type;

	private ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier;

	private BracketedParameterListSyntax parameterList;

	private AccessorListSyntax accessorList;

	private ArrowExpressionClauseSyntax expressionBody;

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

	public SyntaxToken ThisKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IndexerDeclarationSyntax)Green).thisKeyword, GetChildPosition(4), GetChildIndex(4));

	public BracketedParameterListSyntax ParameterList => GetRed(ref parameterList, 5);

	public override AccessorListSyntax AccessorList => GetRed(ref accessorList, 6);

	public ArrowExpressionClauseSyntax ExpressionBody => GetRed(ref expressionBody, 7);

	public SyntaxToken SemicolonToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken semicolonToken = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IndexerDeclarationSyntax)Green).semicolonToken;
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

	internal IndexerDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
			5 => GetRed(ref parameterList, 5), 
			6 => GetRed(ref accessorList, 6), 
			7 => GetRed(ref expressionBody, 7), 
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
			5 => parameterList, 
			6 => accessorList, 
			7 => expressionBody, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIndexerDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitIndexerDeclaration(this);
	}

	public IndexerDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken thisKeyword, BracketedParameterListSyntax parameterList, AccessorListSyntax accessorList, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || type != Type || explicitInterfaceSpecifier != ExplicitInterfaceSpecifier || thisKeyword != ThisKeyword || parameterList != ParameterList || accessorList != AccessorList || expressionBody != ExpressionBody || semicolonToken != SemicolonToken)
		{
			IndexerDeclarationSyntax indexerDeclarationSyntax = SyntaxFactory.IndexerDeclaration(attributeLists, modifiers, type, explicitInterfaceSpecifier, thisKeyword, parameterList, accessorList, expressionBody, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return indexerDeclarationSyntax.WithAnnotations(annotations);
			}
			return indexerDeclarationSyntax;
		}
		return this;
	}

	internal override BasePropertyDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeLists(attributeLists);
	}

	public new IndexerDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, Type, ExplicitInterfaceSpecifier, ThisKeyword, ParameterList, AccessorList, ExpressionBody, SemicolonToken);
	}

	internal override BasePropertyDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers)
	{
		return WithModifiers(modifiers);
	}

	public new IndexerDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, Type, ExplicitInterfaceSpecifier, ThisKeyword, ParameterList, AccessorList, ExpressionBody, SemicolonToken);
	}

	internal override BasePropertyDeclarationSyntax WithTypeCore(TypeSyntax type)
	{
		return WithType(type);
	}

	public new IndexerDeclarationSyntax WithType(TypeSyntax type)
	{
		return Update(AttributeLists, Modifiers, type, ExplicitInterfaceSpecifier, ThisKeyword, ParameterList, AccessorList, ExpressionBody, SemicolonToken);
	}

	internal override BasePropertyDeclarationSyntax WithExplicitInterfaceSpecifierCore(ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier)
	{
		return WithExplicitInterfaceSpecifier(explicitInterfaceSpecifier);
	}

	public new IndexerDeclarationSyntax WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier)
	{
		return Update(AttributeLists, Modifiers, Type, explicitInterfaceSpecifier, ThisKeyword, ParameterList, AccessorList, ExpressionBody, SemicolonToken);
	}

	public IndexerDeclarationSyntax WithThisKeyword(SyntaxToken thisKeyword)
	{
		return Update(AttributeLists, Modifiers, Type, ExplicitInterfaceSpecifier, thisKeyword, ParameterList, AccessorList, ExpressionBody, SemicolonToken);
	}

	public IndexerDeclarationSyntax WithParameterList(BracketedParameterListSyntax parameterList)
	{
		return Update(AttributeLists, Modifiers, Type, ExplicitInterfaceSpecifier, ThisKeyword, parameterList, AccessorList, ExpressionBody, SemicolonToken);
	}

	internal override BasePropertyDeclarationSyntax WithAccessorListCore(AccessorListSyntax accessorList)
	{
		return WithAccessorList(accessorList);
	}

	public new IndexerDeclarationSyntax WithAccessorList(AccessorListSyntax accessorList)
	{
		return Update(AttributeLists, Modifiers, Type, ExplicitInterfaceSpecifier, ThisKeyword, ParameterList, accessorList, ExpressionBody, SemicolonToken);
	}

	public IndexerDeclarationSyntax WithExpressionBody(ArrowExpressionClauseSyntax expressionBody)
	{
		return Update(AttributeLists, Modifiers, Type, ExplicitInterfaceSpecifier, ThisKeyword, ParameterList, AccessorList, expressionBody, SemicolonToken);
	}

	public IndexerDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(AttributeLists, Modifiers, Type, ExplicitInterfaceSpecifier, ThisKeyword, ParameterList, AccessorList, ExpressionBody, semicolonToken);
	}

	internal override BasePropertyDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items)
	{
		return AddAttributeLists(items);
	}

	public new IndexerDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	internal override BasePropertyDeclarationSyntax AddModifiersCore(params SyntaxToken[] items)
	{
		return AddModifiers(items);
	}

	public new IndexerDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	public IndexerDeclarationSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		return WithParameterList(ParameterList.WithParameters(ParameterList.Parameters.AddRange(items)));
	}

	internal override BasePropertyDeclarationSyntax AddAccessorListAccessorsCore(params AccessorDeclarationSyntax[] items)
	{
		return AddAccessorListAccessors(items);
	}

	public new IndexerDeclarationSyntax AddAccessorListAccessors(params AccessorDeclarationSyntax[] items)
	{
		AccessorListSyntax accessorListSyntax = AccessorList ?? SyntaxFactory.AccessorList();
		return WithAccessorList(accessorListSyntax.WithAccessors(accessorListSyntax.Accessors.AddRange(items)));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("This member is obsolete.", true)]
	public IndexerDeclarationSyntax WithSemicolon(SyntaxToken semicolon)
	{
		return WithSemicolonToken(semicolon);
	}
}
