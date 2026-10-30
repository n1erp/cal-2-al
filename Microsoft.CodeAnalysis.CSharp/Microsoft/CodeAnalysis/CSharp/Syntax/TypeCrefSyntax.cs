using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class TypeCrefSyntax : CrefSyntax
{
	private TypeSyntax type;

	public TypeSyntax Type => GetRedAtZero(ref type);

	internal TypeCrefSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitTypeCref(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTypeCref(this);
	}

	public TypeCrefSyntax Update(TypeSyntax type)
	{
		if (type != Type)
		{
			TypeCrefSyntax typeCrefSyntax = SyntaxFactory.TypeCref(type);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return typeCrefSyntax.WithAnnotations(annotations);
			}
			return typeCrefSyntax;
		}
		return this;
	}

	public TypeCrefSyntax WithType(TypeSyntax type)
	{
		return Update(type);
	}
}
