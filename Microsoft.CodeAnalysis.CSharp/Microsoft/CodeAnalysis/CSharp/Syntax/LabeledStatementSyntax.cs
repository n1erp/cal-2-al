using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class LabeledStatementSyntax : StatementSyntax
{
	private StatementSyntax statement;

	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LabeledStatementSyntax)Green).identifier, Position, 0);

	public SyntaxToken ColonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LabeledStatementSyntax)Green).colonToken, GetChildPosition(1), GetChildIndex(1));

	public StatementSyntax Statement => GetRed(ref statement, 2);

	internal LabeledStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref statement, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return statement;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLabeledStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLabeledStatement(this);
	}

	public LabeledStatementSyntax Update(SyntaxToken identifier, SyntaxToken colonToken, StatementSyntax statement)
	{
		if (identifier != Identifier || colonToken != ColonToken || statement != Statement)
		{
			LabeledStatementSyntax labeledStatementSyntax = SyntaxFactory.LabeledStatement(identifier, colonToken, statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return labeledStatementSyntax.WithAnnotations(annotations);
			}
			return labeledStatementSyntax;
		}
		return this;
	}

	public LabeledStatementSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(identifier, ColonToken, Statement);
	}

	public LabeledStatementSyntax WithColonToken(SyntaxToken colonToken)
	{
		return Update(Identifier, colonToken, Statement);
	}

	public LabeledStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(Identifier, ColonToken, statement);
	}
}
