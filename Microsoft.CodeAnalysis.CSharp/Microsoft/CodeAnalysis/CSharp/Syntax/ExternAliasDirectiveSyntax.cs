using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ExternAliasDirectiveSyntax : CSharpSyntaxNode
{
	public SyntaxToken ExternKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExternAliasDirectiveSyntax)Green).externKeyword, Position, 0);

	public SyntaxToken AliasKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExternAliasDirectiveSyntax)Green).aliasKeyword, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExternAliasDirectiveSyntax)Green).identifier, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExternAliasDirectiveSyntax)Green).semicolonToken, GetChildPosition(3), GetChildIndex(3));

	internal ExternAliasDirectiveSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitExternAliasDirective(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitExternAliasDirective(this);
	}

	public ExternAliasDirectiveSyntax Update(SyntaxToken externKeyword, SyntaxToken aliasKeyword, SyntaxToken identifier, SyntaxToken semicolonToken)
	{
		if (externKeyword != ExternKeyword || aliasKeyword != AliasKeyword || identifier != Identifier || semicolonToken != SemicolonToken)
		{
			ExternAliasDirectiveSyntax externAliasDirectiveSyntax = SyntaxFactory.ExternAliasDirective(externKeyword, aliasKeyword, identifier, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return externAliasDirectiveSyntax.WithAnnotations(annotations);
			}
			return externAliasDirectiveSyntax;
		}
		return this;
	}

	public ExternAliasDirectiveSyntax WithExternKeyword(SyntaxToken externKeyword)
	{
		return Update(externKeyword, AliasKeyword, Identifier, SemicolonToken);
	}

	public ExternAliasDirectiveSyntax WithAliasKeyword(SyntaxToken aliasKeyword)
	{
		return Update(ExternKeyword, aliasKeyword, Identifier, SemicolonToken);
	}

	public ExternAliasDirectiveSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(ExternKeyword, AliasKeyword, identifier, SemicolonToken);
	}

	public ExternAliasDirectiveSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(ExternKeyword, AliasKeyword, Identifier, semicolonToken);
	}
}
