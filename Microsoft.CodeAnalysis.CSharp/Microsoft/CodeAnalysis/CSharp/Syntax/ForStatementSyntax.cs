using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ForStatementSyntax : StatementSyntax
{
	private VariableDeclarationSyntax declaration;

	private SyntaxNode initializers;

	private ExpressionSyntax condition;

	private SyntaxNode incrementors;

	private StatementSyntax statement;

	public SyntaxToken ForKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ForStatementSyntax)Green).forKeyword, Position, 0);

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ForStatementSyntax)Green).openParenToken, GetChildPosition(1), GetChildIndex(1));

	public VariableDeclarationSyntax Declaration => GetRed(ref declaration, 2);

	public SeparatedSyntaxList<ExpressionSyntax> Initializers
	{
		get
		{
			SyntaxNode red = GetRed(ref initializers, 3);
			if (red != null)
			{
				return new SeparatedSyntaxList<ExpressionSyntax>(red, GetChildIndex(3));
			}
			return default;
		}
	}

	public SyntaxToken FirstSemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ForStatementSyntax)Green).firstSemicolonToken, GetChildPosition(4), GetChildIndex(4));

	public ExpressionSyntax Condition => GetRed(ref condition, 5);

	public SyntaxToken SecondSemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ForStatementSyntax)Green).secondSemicolonToken, GetChildPosition(6), GetChildIndex(6));

	public SeparatedSyntaxList<ExpressionSyntax> Incrementors
	{
		get
		{
			SyntaxNode red = GetRed(ref incrementors, 7);
			if (red != null)
			{
				return new SeparatedSyntaxList<ExpressionSyntax>(red, GetChildIndex(7));
			}
			return default;
		}
	}

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ForStatementSyntax)Green).closeParenToken, GetChildPosition(8), GetChildIndex(8));

	public StatementSyntax Statement => GetRed(ref statement, 9);

	internal ForStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref declaration, 2), 
			3 => GetRed(ref initializers, 3), 
			5 => GetRed(ref condition, 5), 
			7 => GetRed(ref incrementors, 7), 
			9 => GetRed(ref statement, 9), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => declaration, 
			3 => initializers, 
			5 => condition, 
			7 => incrementors, 
			9 => statement, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitForStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitForStatement(this);
	}

	public ForStatementSyntax Update(SyntaxToken forKeyword, SyntaxToken openParenToken, VariableDeclarationSyntax declaration, SeparatedSyntaxList<ExpressionSyntax> initializers, SyntaxToken firstSemicolonToken, ExpressionSyntax condition, SyntaxToken secondSemicolonToken, SeparatedSyntaxList<ExpressionSyntax> incrementors, SyntaxToken closeParenToken, StatementSyntax statement)
	{
		if (forKeyword != ForKeyword || openParenToken != OpenParenToken || declaration != Declaration || initializers != Initializers || firstSemicolonToken != FirstSemicolonToken || condition != Condition || secondSemicolonToken != SecondSemicolonToken || incrementors != Incrementors || closeParenToken != CloseParenToken || statement != Statement)
		{
			ForStatementSyntax forStatementSyntax = SyntaxFactory.ForStatement(forKeyword, openParenToken, declaration, initializers, firstSemicolonToken, condition, secondSemicolonToken, incrementors, closeParenToken, statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return forStatementSyntax.WithAnnotations(annotations);
			}
			return forStatementSyntax;
		}
		return this;
	}

	public ForStatementSyntax WithForKeyword(SyntaxToken forKeyword)
	{
		return Update(forKeyword, OpenParenToken, Declaration, Initializers, FirstSemicolonToken, Condition, SecondSemicolonToken, Incrementors, CloseParenToken, Statement);
	}

	public ForStatementSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(ForKeyword, openParenToken, Declaration, Initializers, FirstSemicolonToken, Condition, SecondSemicolonToken, Incrementors, CloseParenToken, Statement);
	}

	public ForStatementSyntax WithDeclaration(VariableDeclarationSyntax declaration)
	{
		return Update(ForKeyword, OpenParenToken, declaration, Initializers, FirstSemicolonToken, Condition, SecondSemicolonToken, Incrementors, CloseParenToken, Statement);
	}

	public ForStatementSyntax WithInitializers(SeparatedSyntaxList<ExpressionSyntax> initializers)
	{
		return Update(ForKeyword, OpenParenToken, Declaration, initializers, FirstSemicolonToken, Condition, SecondSemicolonToken, Incrementors, CloseParenToken, Statement);
	}

	public ForStatementSyntax WithFirstSemicolonToken(SyntaxToken firstSemicolonToken)
	{
		return Update(ForKeyword, OpenParenToken, Declaration, Initializers, firstSemicolonToken, Condition, SecondSemicolonToken, Incrementors, CloseParenToken, Statement);
	}

	public ForStatementSyntax WithCondition(ExpressionSyntax condition)
	{
		return Update(ForKeyword, OpenParenToken, Declaration, Initializers, FirstSemicolonToken, condition, SecondSemicolonToken, Incrementors, CloseParenToken, Statement);
	}

	public ForStatementSyntax WithSecondSemicolonToken(SyntaxToken secondSemicolonToken)
	{
		return Update(ForKeyword, OpenParenToken, Declaration, Initializers, FirstSemicolonToken, Condition, secondSemicolonToken, Incrementors, CloseParenToken, Statement);
	}

	public ForStatementSyntax WithIncrementors(SeparatedSyntaxList<ExpressionSyntax> incrementors)
	{
		return Update(ForKeyword, OpenParenToken, Declaration, Initializers, FirstSemicolonToken, Condition, SecondSemicolonToken, incrementors, CloseParenToken, Statement);
	}

	public ForStatementSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(ForKeyword, OpenParenToken, Declaration, Initializers, FirstSemicolonToken, Condition, SecondSemicolonToken, Incrementors, closeParenToken, Statement);
	}

	public ForStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(ForKeyword, OpenParenToken, Declaration, Initializers, FirstSemicolonToken, Condition, SecondSemicolonToken, Incrementors, CloseParenToken, statement);
	}

	public ForStatementSyntax AddInitializers(params ExpressionSyntax[] items)
	{
		return WithInitializers(Initializers.AddRange(items));
	}

	public ForStatementSyntax AddIncrementors(params ExpressionSyntax[] items)
	{
		return WithIncrementors(Incrementors.AddRange(items));
	}
}
