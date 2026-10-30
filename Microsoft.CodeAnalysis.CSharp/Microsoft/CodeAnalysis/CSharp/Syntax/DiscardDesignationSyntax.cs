using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class DiscardDesignationSyntax : VariableDesignationSyntax
{
	public SyntaxToken UnderscoreToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.DiscardDesignationSyntax)Green).underscoreToken, Position, 0);

	internal DiscardDesignationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitDiscardDesignation(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDiscardDesignation(this);
	}

	public DiscardDesignationSyntax Update(SyntaxToken underscoreToken)
	{
		if (underscoreToken != UnderscoreToken)
		{
			DiscardDesignationSyntax discardDesignationSyntax = SyntaxFactory.DiscardDesignation(underscoreToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return discardDesignationSyntax.WithAnnotations(annotations);
			}
			return discardDesignationSyntax;
		}
		return this;
	}

	public DiscardDesignationSyntax WithUnderscoreToken(SyntaxToken underscoreToken)
	{
		return Update(underscoreToken);
	}
}
