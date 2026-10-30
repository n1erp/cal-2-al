using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class IncompleteMemberSyntax : MemberDeclarationSyntax
{
	private SyntaxNode attributeLists;

	private TypeSyntax type;

	public SyntaxList<AttributeListSyntax> AttributeLists => new SyntaxList<AttributeListSyntax>(GetRed(ref attributeLists, 0));

	public SyntaxTokenList Modifiers
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

	public TypeSyntax Type => GetRed(ref type, 2);

	internal IncompleteMemberSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			2 => GetRed(ref type, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			2 => type, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIncompleteMember(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitIncompleteMember(this);
	}

	public IncompleteMemberSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, TypeSyntax type)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || type != Type)
		{
			IncompleteMemberSyntax incompleteMemberSyntax = SyntaxFactory.IncompleteMember(attributeLists, modifiers, type);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return incompleteMemberSyntax.WithAnnotations(annotations);
			}
			return incompleteMemberSyntax;
		}
		return this;
	}

	public IncompleteMemberSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, Type);
	}

	public IncompleteMemberSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, Type);
	}

	public IncompleteMemberSyntax WithType(TypeSyntax type)
	{
		return Update(AttributeLists, Modifiers, type);
	}

	public IncompleteMemberSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	public IncompleteMemberSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}
}
