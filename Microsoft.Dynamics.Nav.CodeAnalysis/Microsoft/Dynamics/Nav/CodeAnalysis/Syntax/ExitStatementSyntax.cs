using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ExitStatementSyntax : StatementSyntax
{
	private CodeExpressionSyntax _exitValue;

	public SyntaxToken ExitKeywordToken => new SyntaxToken(this, ((InternalExitStatementSyntax)Node)._exitKeywordToken, Position, 0);

	public SyntaxToken OpenParenthesisToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalExitStatementSyntax)Node)._openParenthesisToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public CodeExpressionSyntax ExitValue => GetRed(ref _exitValue, 2);

	public SyntaxToken CloseParenthesisToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalExitStatementSyntax)Node)._closeParenthesisToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(3), GetChildIndex(3));
			}
			return default;
		}
	}

	public SyntaxToken SemicolonToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalExitStatementSyntax)Node)._semicolonToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(4), GetChildIndex(4));
			}
			return default;
		}
	}

	internal ExitStatementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _exitValue, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _exitValue;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitExitStatement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitExitStatement(this);
	}

	public ExitStatementSyntax Update(SyntaxToken exitKeywordToken, SyntaxToken openParenthesisToken, CodeExpressionSyntax exitValue, SyntaxToken closeParenthesisToken, SyntaxToken semicolonToken)
	{
		if (exitKeywordToken != ExitKeywordToken || openParenthesisToken != OpenParenthesisToken || exitValue != ExitValue || closeParenthesisToken != CloseParenthesisToken || semicolonToken != SemicolonToken)
		{
			ExitStatementSyntax exitStatementSyntax = SyntaxFactory.ExitStatement(exitKeywordToken, openParenthesisToken, exitValue, closeParenthesisToken, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return exitStatementSyntax.WithAnnotations(annotations);
			}
			return exitStatementSyntax;
		}
		return this;
	}

	public ExitStatementSyntax WithExitKeywordToken(SyntaxToken exitKeywordToken)
	{
		return Update(exitKeywordToken, OpenParenthesisToken, ExitValue, CloseParenthesisToken, SemicolonToken);
	}

	public ExitStatementSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ExitKeywordToken, openParenthesisToken, ExitValue, CloseParenthesisToken, SemicolonToken);
	}

	public ExitStatementSyntax WithExitValue(CodeExpressionSyntax exitValue)
	{
		return Update(ExitKeywordToken, OpenParenthesisToken, exitValue, CloseParenthesisToken, SemicolonToken);
	}

	public ExitStatementSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ExitKeywordToken, OpenParenthesisToken, ExitValue, closeParenthesisToken, SemicolonToken);
	}

	public ExitStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(ExitKeywordToken, OpenParenthesisToken, ExitValue, CloseParenthesisToken, semicolonToken);
	}
}
