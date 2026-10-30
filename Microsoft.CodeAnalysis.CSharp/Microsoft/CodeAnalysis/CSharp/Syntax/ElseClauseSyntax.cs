using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ElseClauseSyntax : CSharpSyntaxNode
{
	private StatementSyntax statement;

	public SyntaxToken ElseKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ElseClauseSyntax)Green).elseKeyword, Position, 0);

	public StatementSyntax Statement => GetRed(ref statement, 1);

	internal ElseClauseSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref statement, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return statement;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitElseClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitElseClause(this);
	}

	public ElseClauseSyntax Update(SyntaxToken elseKeyword, StatementSyntax statement)
	{
		if (elseKeyword != ElseKeyword || statement != Statement)
		{
			ElseClauseSyntax elseClauseSyntax = SyntaxFactory.ElseClause(elseKeyword, statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return elseClauseSyntax.WithAnnotations(annotations);
			}
			return elseClauseSyntax;
		}
		return this;
	}

	public ElseClauseSyntax WithElseKeyword(SyntaxToken elseKeyword)
	{
		return Update(elseKeyword, Statement);
	}

	public ElseClauseSyntax WithStatement(StatementSyntax statement)
	{
		return Update(ElseKeyword, statement);
	}
}
