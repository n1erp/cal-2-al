using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ClassOrStructConstraintSyntax : TypeParameterConstraintSyntax
{
	public SyntaxToken ClassOrStructKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ClassOrStructConstraintSyntax)Green).classOrStructKeyword, Position, 0);

	internal ClassOrStructConstraintSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitClassOrStructConstraint(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitClassOrStructConstraint(this);
	}

	public ClassOrStructConstraintSyntax Update(SyntaxToken classOrStructKeyword)
	{
		if (classOrStructKeyword != ClassOrStructKeyword)
		{
			ClassOrStructConstraintSyntax classOrStructConstraintSyntax = SyntaxFactory.ClassOrStructConstraint(Kind(), classOrStructKeyword);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return classOrStructConstraintSyntax.WithAnnotations(annotations);
			}
			return classOrStructConstraintSyntax;
		}
		return this;
	}

	public ClassOrStructConstraintSyntax WithClassOrStructKeyword(SyntaxToken classOrStructKeyword)
	{
		return Update(classOrStructKeyword);
	}
}
