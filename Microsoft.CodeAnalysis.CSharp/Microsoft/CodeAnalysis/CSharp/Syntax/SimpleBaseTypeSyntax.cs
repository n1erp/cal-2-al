using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class SimpleBaseTypeSyntax : BaseTypeSyntax
{
	private TypeSyntax type;

	public override TypeSyntax Type => GetRedAtZero(ref type);

	internal SimpleBaseTypeSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref type);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return type;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSimpleBaseType(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSimpleBaseType(this);
	}

	public SimpleBaseTypeSyntax Update(TypeSyntax type)
	{
		if (type != Type)
		{
			SimpleBaseTypeSyntax simpleBaseTypeSyntax = SyntaxFactory.SimpleBaseType(type);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return simpleBaseTypeSyntax.WithAnnotations(annotations);
			}
			return simpleBaseTypeSyntax;
		}
		return this;
	}

	internal override BaseTypeSyntax WithTypeCore(TypeSyntax type)
	{
		return WithType(type);
	}

	public new SimpleBaseTypeSyntax WithType(TypeSyntax type)
	{
		return Update(type);
	}
}
