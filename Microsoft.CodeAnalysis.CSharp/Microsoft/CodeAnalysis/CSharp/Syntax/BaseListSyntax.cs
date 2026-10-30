using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class BaseListSyntax : CSharpSyntaxNode
{
	private SyntaxNode types;

	public SyntaxToken ColonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BaseListSyntax)Green).colonToken, Position, 0);

	public SeparatedSyntaxList<BaseTypeSyntax> Types
	{
		get
		{
			SyntaxNode red = GetRed(ref types, 1);
			if (red != null)
			{
				return new SeparatedSyntaxList<BaseTypeSyntax>(red, GetChildIndex(1));
			}
			return default;
		}
	}

	internal BaseListSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref types, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return types;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBaseList(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitBaseList(this);
	}

	public BaseListSyntax Update(SyntaxToken colonToken, SeparatedSyntaxList<BaseTypeSyntax> types)
	{
		if (colonToken != ColonToken || types != Types)
		{
			BaseListSyntax baseListSyntax = SyntaxFactory.BaseList(colonToken, types);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return baseListSyntax.WithAnnotations(annotations);
			}
			return baseListSyntax;
		}
		return this;
	}

	public BaseListSyntax WithColonToken(SyntaxToken colonToken)
	{
		return Update(colonToken, Types);
	}

	public BaseListSyntax WithTypes(SeparatedSyntaxList<BaseTypeSyntax> types)
	{
		return Update(ColonToken, types);
	}

	public BaseListSyntax AddTypes(params BaseTypeSyntax[] items)
	{
		return WithTypes(Types.AddRange(items));
	}
}
