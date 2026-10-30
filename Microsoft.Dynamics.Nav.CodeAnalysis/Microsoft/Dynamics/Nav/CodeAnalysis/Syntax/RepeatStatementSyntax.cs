using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class RepeatStatementSyntax : StatementSyntax
{
	private SyntaxNode _statements;

	private CodeExpressionSyntax _condition;

	public SyntaxToken RepeatKeywordToken => new SyntaxToken(this, ((InternalRepeatStatementSyntax)Node)._repeatKeywordToken, Position, 0);

	public SyntaxList<StatementSyntax> Statements => new SyntaxList<StatementSyntax>(GetRed(ref _statements, 1));

	public SyntaxToken UntilKeywordToken => new SyntaxToken(this, ((InternalRepeatStatementSyntax)Node)._untilKeywordToken, GetChildPosition(2), GetChildIndex(2));

	public CodeExpressionSyntax Condition => GetRed(ref _condition, 3);

	public SyntaxToken SemicolonToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalRepeatStatementSyntax)Node)._semicolonToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(4), GetChildIndex(4));
			}
			return default;
		}
	}

	internal RepeatStatementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref _statements, 1), 
			3 => GetRed(ref _condition, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => _statements, 
			3 => _condition, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRepeatStatement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitRepeatStatement(this);
	}

	public RepeatStatementSyntax Update(SyntaxToken repeatKeywordToken, SyntaxList<StatementSyntax> statements, SyntaxToken untilKeywordToken, CodeExpressionSyntax condition, SyntaxToken semicolonToken)
	{
		if (repeatKeywordToken != RepeatKeywordToken || statements != Statements || untilKeywordToken != UntilKeywordToken || condition != Condition || semicolonToken != SemicolonToken)
		{
			RepeatStatementSyntax repeatStatementSyntax = SyntaxFactory.RepeatStatement(repeatKeywordToken, statements, untilKeywordToken, condition, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return repeatStatementSyntax.WithAnnotations(annotations);
			}
			return repeatStatementSyntax;
		}
		return this;
	}

	public RepeatStatementSyntax WithRepeatKeywordToken(SyntaxToken repeatKeywordToken)
	{
		return Update(repeatKeywordToken, Statements, UntilKeywordToken, Condition, SemicolonToken);
	}

	public RepeatStatementSyntax WithStatements(SyntaxList<StatementSyntax> statements)
	{
		return Update(RepeatKeywordToken, statements, UntilKeywordToken, Condition, SemicolonToken);
	}

	public RepeatStatementSyntax WithUntilKeywordToken(SyntaxToken untilKeywordToken)
	{
		return Update(RepeatKeywordToken, Statements, untilKeywordToken, Condition, SemicolonToken);
	}

	public RepeatStatementSyntax WithCondition(CodeExpressionSyntax condition)
	{
		return Update(RepeatKeywordToken, Statements, UntilKeywordToken, condition, SemicolonToken);
	}

	public RepeatStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(RepeatKeywordToken, Statements, UntilKeywordToken, Condition, semicolonToken);
	}

	public RepeatStatementSyntax AddStatements(params StatementSyntax[] items)
	{
		return WithStatements(Statements.AddRange(items));
	}
}
