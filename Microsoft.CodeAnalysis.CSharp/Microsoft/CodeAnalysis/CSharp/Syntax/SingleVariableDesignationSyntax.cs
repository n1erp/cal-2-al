using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class SingleVariableDesignationSyntax : VariableDesignationSyntax
{
	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SingleVariableDesignationSyntax)Green).identifier, Position, 0);

	internal SingleVariableDesignationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitSingleVariableDesignation(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSingleVariableDesignation(this);
	}

	public SingleVariableDesignationSyntax Update(SyntaxToken identifier)
	{
		if (identifier != Identifier)
		{
			SingleVariableDesignationSyntax singleVariableDesignationSyntax = SyntaxFactory.SingleVariableDesignation(identifier);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return singleVariableDesignationSyntax.WithAnnotations(annotations);
			}
			return singleVariableDesignationSyntax;
		}
		return this;
	}

	public SingleVariableDesignationSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(identifier);
	}
}
