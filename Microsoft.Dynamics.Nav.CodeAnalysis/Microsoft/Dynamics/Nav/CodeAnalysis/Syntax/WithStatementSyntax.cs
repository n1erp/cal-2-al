using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class WithStatementSyntax : StatementSyntax
{
	private CodeExpressionSyntax _withId;

	private StatementSyntax _statement;

	public SyntaxToken WithKeywordToken => new SyntaxToken(this, ((InternalWithStatementSyntax)Node)._withKeywordToken, Position, 0);

	public CodeExpressionSyntax WithId => GetRed(ref _withId, 1);

	public SyntaxToken DoKeywordToken => new SyntaxToken(this, ((InternalWithStatementSyntax)Node)._doKeywordToken, GetChildPosition(2), GetChildIndex(2));

	public StatementSyntax Statement => GetRed(ref _statement, 3);

	internal WithStatementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)GetRed(ref _withId, 1), 
			3 => GetRed(ref _statement, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)_withId, 
			3 => _statement, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitWithStatement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitWithStatement(this);
	}

	public WithStatementSyntax Update(SyntaxToken withKeywordToken, CodeExpressionSyntax withId, SyntaxToken doKeywordToken, StatementSyntax statement)
	{
		if (withKeywordToken != WithKeywordToken || withId != WithId || doKeywordToken != DoKeywordToken || statement != Statement)
		{
			WithStatementSyntax withStatementSyntax = SyntaxFactory.WithStatement(withKeywordToken, withId, doKeywordToken, statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return withStatementSyntax.WithAnnotations(annotations);
			}
			return withStatementSyntax;
		}
		return this;
	}

	public WithStatementSyntax WithWithKeywordToken(SyntaxToken withKeywordToken)
	{
		return Update(withKeywordToken, WithId, DoKeywordToken, Statement);
	}

	public WithStatementSyntax WithWithId(CodeExpressionSyntax withId)
	{
		return Update(WithKeywordToken, withId, DoKeywordToken, Statement);
	}

	public WithStatementSyntax WithDoKeywordToken(SyntaxToken doKeywordToken)
	{
		return Update(WithKeywordToken, WithId, doKeywordToken, Statement);
	}

	public WithStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(WithKeywordToken, WithId, DoKeywordToken, statement);
	}
}
