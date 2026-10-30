using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class OrphanedElseStatementSyntax : StatementSyntax
{
	private StatementSyntax _elseStatement;

	public SyntaxToken ElseKeywordToken => new SyntaxToken(this, ((InternalOrphanedElseStatementSyntax)Node)._elseKeywordToken, Position, 0);

	public StatementSyntax ElseStatement => GetRed(ref _elseStatement, 1);

	internal OrphanedElseStatementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _elseStatement, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _elseStatement;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOrphanedElseStatement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitOrphanedElseStatement(this);
	}

	public OrphanedElseStatementSyntax Update(SyntaxToken elseKeywordToken, StatementSyntax elseStatement)
	{
		if (elseKeywordToken != ElseKeywordToken || elseStatement != ElseStatement)
		{
			OrphanedElseStatementSyntax orphanedElseStatementSyntax = SyntaxFactory.OrphanedElseStatement(elseKeywordToken, elseStatement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return orphanedElseStatementSyntax.WithAnnotations(annotations);
			}
			return orphanedElseStatementSyntax;
		}
		return this;
	}

	public OrphanedElseStatementSyntax WithElseKeywordToken(SyntaxToken elseKeywordToken)
	{
		return Update(elseKeywordToken, ElseStatement);
	}

	public OrphanedElseStatementSyntax WithElseStatement(StatementSyntax elseStatement)
	{
		return Update(ElseKeywordToken, elseStatement);
	}
}
