using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class PointerTypeSyntax : TypeSyntax
{
	private TypeSyntax elementType;

	public TypeSyntax ElementType => GetRedAtZero(ref elementType);

	public SyntaxToken AsteriskToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PointerTypeSyntax)Green).asteriskToken, GetChildPosition(1), GetChildIndex(1));

	internal PointerTypeSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref elementType);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return elementType;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPointerType(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitPointerType(this);
	}

	public PointerTypeSyntax Update(TypeSyntax elementType, SyntaxToken asteriskToken)
	{
		if (elementType != ElementType || asteriskToken != AsteriskToken)
		{
			PointerTypeSyntax pointerTypeSyntax = SyntaxFactory.PointerType(elementType, asteriskToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pointerTypeSyntax.WithAnnotations(annotations);
			}
			return pointerTypeSyntax;
		}
		return this;
	}

	public PointerTypeSyntax WithElementType(TypeSyntax elementType)
	{
		return Update(elementType, AsteriskToken);
	}

	public PointerTypeSyntax WithAsteriskToken(SyntaxToken asteriskToken)
	{
		return Update(ElementType, asteriskToken);
	}
}
