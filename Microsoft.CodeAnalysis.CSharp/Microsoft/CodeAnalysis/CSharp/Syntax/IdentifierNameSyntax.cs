using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class IdentifierNameSyntax : SimpleNameSyntax
{
	public override SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IdentifierNameSyntax)Green).identifier, Position, 0);

	internal IdentifierNameSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitIdentifierName(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitIdentifierName(this);
	}

	public IdentifierNameSyntax Update(SyntaxToken identifier)
	{
		if (identifier != Identifier)
		{
			IdentifierNameSyntax identifierNameSyntax = SyntaxFactory.IdentifierName(identifier);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return identifierNameSyntax.WithAnnotations(annotations);
			}
			return identifierNameSyntax;
		}
		return this;
	}

	internal override SimpleNameSyntax WithIdentifierCore(SyntaxToken identifier)
	{
		return WithIdentifier(identifier);
	}

	public new IdentifierNameSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(identifier);
	}

	internal override string ErrorDisplayName()
	{
		return Identifier.ValueText;
	}
}
