using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class EnumMemberDeclarationSyntax : MemberDeclarationSyntax
{
	private SyntaxNode attributeLists;

	private EqualsValueClauseSyntax equalsValue;

	public SyntaxList<AttributeListSyntax> AttributeLists => new SyntaxList<AttributeListSyntax>(GetRed(ref attributeLists, 0));

	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.EnumMemberDeclarationSyntax)Green).identifier, GetChildPosition(1), GetChildIndex(1));

	public EqualsValueClauseSyntax EqualsValue => GetRed(ref equalsValue, 2);

	internal EnumMemberDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			2 => GetRed(ref equalsValue, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			2 => equalsValue, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEnumMemberDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitEnumMemberDeclaration(this);
	}

	public EnumMemberDeclarationSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxToken identifier, EqualsValueClauseSyntax equalsValue)
	{
		if (attributeLists != AttributeLists || identifier != Identifier || equalsValue != EqualsValue)
		{
			EnumMemberDeclarationSyntax enumMemberDeclarationSyntax = SyntaxFactory.EnumMemberDeclaration(attributeLists, identifier, equalsValue);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return enumMemberDeclarationSyntax.WithAnnotations(annotations);
			}
			return enumMemberDeclarationSyntax;
		}
		return this;
	}

	public EnumMemberDeclarationSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Identifier, EqualsValue);
	}

	public EnumMemberDeclarationSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(AttributeLists, identifier, EqualsValue);
	}

	public EnumMemberDeclarationSyntax WithEqualsValue(EqualsValueClauseSyntax equalsValue)
	{
		return Update(AttributeLists, Identifier, equalsValue);
	}

	public EnumMemberDeclarationSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}
}
