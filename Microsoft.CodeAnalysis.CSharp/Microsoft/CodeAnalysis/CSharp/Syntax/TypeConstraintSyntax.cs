using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class TypeConstraintSyntax : TypeParameterConstraintSyntax
{
	private TypeSyntax type;

	public TypeSyntax Type => GetRedAtZero(ref type);

	internal TypeConstraintSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitTypeConstraint(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTypeConstraint(this);
	}

	public TypeConstraintSyntax Update(TypeSyntax type)
	{
		if (type != Type)
		{
			TypeConstraintSyntax typeConstraintSyntax = SyntaxFactory.TypeConstraint(type);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return typeConstraintSyntax.WithAnnotations(annotations);
			}
			return typeConstraintSyntax;
		}
		return this;
	}

	public TypeConstraintSyntax WithType(TypeSyntax type)
	{
		return Update(type);
	}
}
