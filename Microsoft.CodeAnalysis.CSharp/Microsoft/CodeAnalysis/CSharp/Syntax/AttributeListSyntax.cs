using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class AttributeListSyntax : CSharpSyntaxNode
{
	private AttributeTargetSpecifierSyntax target;

	private SyntaxNode attributes;

	public SyntaxToken OpenBracketToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax)Green).openBracketToken, Position, 0);

	public AttributeTargetSpecifierSyntax Target => GetRed(ref target, 1);

	public SeparatedSyntaxList<AttributeSyntax> Attributes
	{
		get
		{
			SyntaxNode red = GetRed(ref attributes, 2);
			if (red != null)
			{
				return new SeparatedSyntaxList<AttributeSyntax>(red, GetChildIndex(2));
			}
			return default;
		}
	}

	public SyntaxToken CloseBracketToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AttributeListSyntax)Green).closeBracketToken, GetChildPosition(3), GetChildIndex(3));

	internal AttributeListSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref target, 1), 
			2 => GetRed(ref attributes, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => target, 
			2 => attributes, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAttributeList(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAttributeList(this);
	}

	public AttributeListSyntax Update(SyntaxToken openBracketToken, AttributeTargetSpecifierSyntax target, SeparatedSyntaxList<AttributeSyntax> attributes, SyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || target != Target || attributes != Attributes || closeBracketToken != CloseBracketToken)
		{
			AttributeListSyntax attributeListSyntax = SyntaxFactory.AttributeList(openBracketToken, target, attributes, closeBracketToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return attributeListSyntax.WithAnnotations(annotations);
			}
			return attributeListSyntax;
		}
		return this;
	}

	public AttributeListSyntax WithOpenBracketToken(SyntaxToken openBracketToken)
	{
		return Update(openBracketToken, Target, Attributes, CloseBracketToken);
	}

	public AttributeListSyntax WithTarget(AttributeTargetSpecifierSyntax target)
	{
		return Update(OpenBracketToken, target, Attributes, CloseBracketToken);
	}

	public AttributeListSyntax WithAttributes(SeparatedSyntaxList<AttributeSyntax> attributes)
	{
		return Update(OpenBracketToken, Target, attributes, CloseBracketToken);
	}

	public AttributeListSyntax WithCloseBracketToken(SyntaxToken closeBracketToken)
	{
		return Update(OpenBracketToken, Target, Attributes, closeBracketToken);
	}

	public AttributeListSyntax AddAttributes(params AttributeSyntax[] items)
	{
		return WithAttributes(Attributes.AddRange(items));
	}
}
