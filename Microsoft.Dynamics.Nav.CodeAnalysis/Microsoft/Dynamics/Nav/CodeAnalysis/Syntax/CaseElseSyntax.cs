using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class CaseElseSyntax : SyntaxNode
{
	private SyntaxNode _elseStatements;

	public SyntaxToken ElseKeywordToken => new SyntaxToken(this, ((InternalCaseElseSyntax)Node)._elseKeywordToken, Position, 0);

	public SyntaxList<StatementSyntax> ElseStatements => new SyntaxList<StatementSyntax>(GetRed(ref _elseStatements, 1));

	internal CaseElseSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _elseStatements, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _elseStatements;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCaseElse(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitCaseElse(this);
	}

	public CaseElseSyntax Update(SyntaxToken elseKeywordToken, SyntaxList<StatementSyntax> elseStatements)
	{
		if (elseKeywordToken != ElseKeywordToken || elseStatements != ElseStatements)
		{
			CaseElseSyntax caseElseSyntax = SyntaxFactory.CaseElse(elseKeywordToken, elseStatements);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return caseElseSyntax.WithAnnotations(annotations);
			}
			return caseElseSyntax;
		}
		return this;
	}

	public CaseElseSyntax WithElseKeywordToken(SyntaxToken elseKeywordToken)
	{
		return Update(elseKeywordToken, ElseStatements);
	}

	public CaseElseSyntax WithElseStatements(SyntaxList<StatementSyntax> elseStatements)
	{
		return Update(ElseKeywordToken, elseStatements);
	}

	public CaseElseSyntax AddElseStatements(params StatementSyntax[] items)
	{
		return WithElseStatements(ElseStatements.AddRange(items));
	}
}
