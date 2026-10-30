using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class FieldDeclarationSyntax : BaseFieldDeclarationSyntax
{
	private SyntaxNode attributeLists;

	private VariableDeclarationSyntax declaration;

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

	public override VariableDeclarationSyntax Declaration => GetRed(ref declaration, 2);

	public override SyntaxToken SemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FieldDeclarationSyntax)Green).semicolonToken, GetChildPosition(3), GetChildIndex(3));

	internal FieldDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			2 => GetRed(ref declaration, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			2 => declaration, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFieldDeclaration(this);
	}

	public FieldDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || declaration != Declaration || semicolonToken != SemicolonToken)
		{
			FieldDeclarationSyntax fieldDeclarationSyntax = SyntaxFactory.FieldDeclaration(attributeLists, modifiers, declaration, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return fieldDeclarationSyntax.WithAnnotations(annotations);
			}
			return fieldDeclarationSyntax;
		}
		return this;
	}

	internal override BaseFieldDeclarationSyntax WithAttributeListsCore(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return WithAttributeLists(attributeLists);
	}

	public new FieldDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, Declaration, SemicolonToken);
	}

	internal override BaseFieldDeclarationSyntax WithModifiersCore(SyntaxTokenList modifiers)
	{
		return WithModifiers(modifiers);
	}

	public new FieldDeclarationSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, Declaration, SemicolonToken);
	}

	internal override BaseFieldDeclarationSyntax WithDeclarationCore(VariableDeclarationSyntax declaration)
	{
		return WithDeclaration(declaration);
	}

	public new FieldDeclarationSyntax WithDeclaration(VariableDeclarationSyntax declaration)
	{
		return Update(AttributeLists, Modifiers, declaration, SemicolonToken);
	}

	internal override BaseFieldDeclarationSyntax WithSemicolonTokenCore(SyntaxToken semicolonToken)
	{
		return WithSemicolonToken(semicolonToken);
	}

	public new FieldDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(AttributeLists, Modifiers, Declaration, semicolonToken);
	}

	internal override BaseFieldDeclarationSyntax AddAttributeListsCore(params AttributeListSyntax[] items)
	{
		return AddAttributeLists(items);
	}

	public new FieldDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	internal override BaseFieldDeclarationSyntax AddModifiersCore(params SyntaxToken[] items)
	{
		return AddModifiers(items);
	}

	public new FieldDeclarationSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	internal override BaseFieldDeclarationSyntax AddDeclarationVariablesCore(params VariableDeclaratorSyntax[] items)
	{
		return AddDeclarationVariables(items);
	}

	public new FieldDeclarationSyntax AddDeclarationVariables(params VariableDeclaratorSyntax[] items)
	{
		return WithDeclaration(Declaration.WithVariables(Declaration.Variables.AddRange(items)));
	}
}
