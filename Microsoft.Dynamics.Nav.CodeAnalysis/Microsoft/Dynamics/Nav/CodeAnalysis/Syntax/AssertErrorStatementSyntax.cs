using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class AssertErrorStatementSyntax : StatementSyntax
{
	private StatementSyntax _statement;

	public SyntaxToken AssertErrorKeywordToken => new SyntaxToken(this, ((InternalAssertErrorStatementSyntax)Node)._assertErrorKeywordToken, Position, 0);

	public StatementSyntax Statement => GetRed(ref _statement, 1);

	internal AssertErrorStatementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _statement, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _statement;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAssertErrorStatement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitAssertErrorStatement(this);
	}

	public AssertErrorStatementSyntax Update(SyntaxToken assertErrorKeywordToken, StatementSyntax statement)
	{
		if (assertErrorKeywordToken != AssertErrorKeywordToken || statement != Statement)
		{
			AssertErrorStatementSyntax assertErrorStatementSyntax = SyntaxFactory.AssertErrorStatement(assertErrorKeywordToken, statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return assertErrorStatementSyntax.WithAnnotations(annotations);
			}
			return assertErrorStatementSyntax;
		}
		return this;
	}

	public AssertErrorStatementSyntax WithAssertErrorKeywordToken(SyntaxToken assertErrorKeywordToken)
	{
		return Update(assertErrorKeywordToken, Statement);
	}

	public AssertErrorStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(AssertErrorKeywordToken, statement);
	}
}
