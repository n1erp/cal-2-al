using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class EventDeclarationSyntax : BasePropertyDeclarationSyntax
{
	private SyntaxNode attributeLists;

	private TypeSyntax type;

	private ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier;

	private AccessorListSyntax accessorList;

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

	public SyntaxToken EventKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EventDeclarationSyntax)Green).eventKeyword, GetChildPosition(2), GetChildIndex(2));

	public override TypeSyntax Type => GetRed(ref type, 3);

	public override ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier => GetRed(ref explicitInterfaceSpecifier, 4);

	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EventDeclarationSyntax)Green).identifier, GetChildPosition(5), GetChildIndex(5));

	public override AccessorListSyntax AccessorList => GetRed(ref accessorList, 6);

	internal EventDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			3 => GetRed(ref type, 3), 
			4 => GetRed(ref explicitInterfaceSpecifier, 4), 
			6 => GetRed(ref accessorList, 6), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			3 => type, 
			4 => explicitInterfaceSpecifier, 
			6 => accessorList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEventDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitEventDeclaration(this);
	}

	public EventDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, SyntaxToken eventKeyword, TypeSyntax type, ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier, SyntaxToken identifier, AccessorListSyntax accessorList)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || eventKeyword != EventKeyword || type != Type || explicitInterfaceSpecifier != ExplicitInterfaceSpecifier || identifier != Identifier || accessorList != AccessorList)
		{
			EventDeclarationSyntax eventDeclarationSyntax = SyntaxFactory.EventDeclaration(attributeLists, modifiers, eventKeyword, type, explicitInterfaceSpecifier, identifier, accessorList);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return eventDeclarationSyntax.WithAnnotations(annotations);
			}
			return eventDeclarationSyntax;
		}
		return this;
	}

	internal override BasePropertyDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeLists(attributeLists);
	}

	public new EventDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, EventKeyword, Type, ExplicitInterfaceSpecifier, Identifier, AccessorList);
	}

	internal override BasePropertyDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers)
	{
		return WithModifiers(modifiers);
	}

	public new EventDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, EventKeyword, Type, ExplicitInterfaceSpecifier, Identifier, AccessorList);
	}

	public EventDeclarationSyntax WithEventKeyword(SyntaxToken eventKeyword)
	{
		return Update(AttributeLists, Modifiers, eventKeyword, Type, ExplicitInterfaceSpecifier, Identifier, AccessorList);
	}

	internal override BasePropertyDeclarationSyntax WithTypeCore(TypeSyntax type)
	{
		return WithType(type);
	}

	public new EventDeclarationSyntax WithType(TypeSyntax type)
	{
		return Update(AttributeLists, Modifiers, EventKeyword, type, ExplicitInterfaceSpecifier, Identifier, AccessorList);
	}

	internal override BasePropertyDeclarationSyntax WithExplicitInterfaceSpecifierCore(ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier)
	{
		return WithExplicitInterfaceSpecifier(explicitInterfaceSpecifier);
	}

	public new EventDeclarationSyntax WithExplicitInterfaceSpecifier(ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier)
	{
		return Update(AttributeLists, Modifiers, EventKeyword, Type, explicitInterfaceSpecifier, Identifier, AccessorList);
	}

	public EventDeclarationSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(AttributeLists, Modifiers, EventKeyword, Type, ExplicitInterfaceSpecifier, identifier, AccessorList);
	}

	internal override BasePropertyDeclarationSyntax WithAccessorListCore(AccessorListSyntax accessorList)
	{
		return WithAccessorList(accessorList);
	}

	public new EventDeclarationSyntax WithAccessorList(AccessorListSyntax accessorList)
	{
		return Update(AttributeLists, Modifiers, EventKeyword, Type, ExplicitInterfaceSpecifier, Identifier, accessorList);
	}

	internal override BasePropertyDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items)
	{
		return AddAttributeLists(items);
	}

	public new EventDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	internal override BasePropertyDeclarationSyntax AddModifiersCore(params SyntaxToken[] items)
	{
		return AddModifiers(items);
	}

	public new EventDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	internal override BasePropertyDeclarationSyntax AddAccessorListAccessorsCore(params AccessorDeclarationSyntax[] items)
	{
		return AddAccessorListAccessors(items);
	}

	public new EventDeclarationSyntax AddAccessorListAccessors(params AccessorDeclarationSyntax[] items)
	{
		return WithAccessorList(AccessorList.WithAccessors(AccessorList.Accessors.AddRange(items)));
	}
}
