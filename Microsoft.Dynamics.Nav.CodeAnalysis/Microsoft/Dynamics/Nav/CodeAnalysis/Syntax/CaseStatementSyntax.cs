using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class CaseStatementSyntax : StatementSyntax
{
	private CodeExpressionSyntax _expression;

	private SyntaxNode _caseLines;

	private CaseElseSyntax _caseElse;

	public SyntaxToken CaseKeywordToken => new SyntaxToken(this, ((InternalCaseStatementSyntax)Node)._caseKeywordToken, Position, 0);

	public CodeExpressionSyntax Expression => GetRed(ref _expression, 1);

	public SyntaxToken OfKeywordToken => new SyntaxToken(this, ((InternalCaseStatementSyntax)Node)._ofKeywordToken, GetChildPosition(2), GetChildIndex(2));

	public SyntaxList<CaseLineSyntax> CaseLines => new SyntaxList<CaseLineSyntax>(GetRed(ref _caseLines, 3));

	public CaseElseSyntax CaseElse => GetRed(ref _caseElse, 4);

	public SyntaxToken EndKeywordToken => new SyntaxToken(this, ((InternalCaseStatementSyntax)Node)._endKeywordToken, GetChildPosition(5), GetChildIndex(5));

	public SyntaxToken SemicolonToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalCaseStatementSyntax)Node)._semicolonToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(6), GetChildIndex(6));
			}
			return default;
		}
	}

	internal CaseStatementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref _expression, 1), 
			3 => GetRed(ref _caseLines, 3), 
			4 => GetRed(ref _caseElse, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => _expression, 
			3 => _caseLines, 
			4 => _caseElse, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCaseStatement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitCaseStatement(this);
	}

	public CaseStatementSyntax Update(SyntaxToken caseKeywordToken, CodeExpressionSyntax expression, SyntaxToken ofKeywordToken, SyntaxList<CaseLineSyntax> caseLines, CaseElseSyntax caseElse, SyntaxToken endKeywordToken, SyntaxToken semicolonToken)
	{
		if (caseKeywordToken != CaseKeywordToken || expression != Expression || ofKeywordToken != OfKeywordToken || caseLines != CaseLines || caseElse != CaseElse || endKeywordToken != EndKeywordToken || semicolonToken != SemicolonToken)
		{
			CaseStatementSyntax caseStatementSyntax = SyntaxFactory.CaseStatement(caseKeywordToken, expression, ofKeywordToken, caseLines, caseElse, endKeywordToken, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return caseStatementSyntax.WithAnnotations(annotations);
			}
			return caseStatementSyntax;
		}
		return this;
	}

	public CaseStatementSyntax WithCaseKeywordToken(SyntaxToken caseKeywordToken)
	{
		return Update(caseKeywordToken, Expression, OfKeywordToken, CaseLines, CaseElse, EndKeywordToken, SemicolonToken);
	}

	public CaseStatementSyntax WithExpression(CodeExpressionSyntax expression)
	{
		return Update(CaseKeywordToken, expression, OfKeywordToken, CaseLines, CaseElse, EndKeywordToken, SemicolonToken);
	}

	public CaseStatementSyntax WithOfKeywordToken(SyntaxToken ofKeywordToken)
	{
		return Update(CaseKeywordToken, Expression, ofKeywordToken, CaseLines, CaseElse, EndKeywordToken, SemicolonToken);
	}

	public CaseStatementSyntax WithCaseLines(SyntaxList<CaseLineSyntax> caseLines)
	{
		return Update(CaseKeywordToken, Expression, OfKeywordToken, caseLines, CaseElse, EndKeywordToken, SemicolonToken);
	}

	public CaseStatementSyntax WithCaseElse(CaseElseSyntax caseElse)
	{
		return Update(CaseKeywordToken, Expression, OfKeywordToken, CaseLines, caseElse, EndKeywordToken, SemicolonToken);
	}

	public CaseStatementSyntax WithEndKeywordToken(SyntaxToken endKeywordToken)
	{
		return Update(CaseKeywordToken, Expression, OfKeywordToken, CaseLines, CaseElse, endKeywordToken, SemicolonToken);
	}

	public CaseStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(CaseKeywordToken, Expression, OfKeywordToken, CaseLines, CaseElse, EndKeywordToken, semicolonToken);
	}

	public CaseStatementSyntax AddCaseLines(params CaseLineSyntax[] items)
	{
		return WithCaseLines(CaseLines.AddRange(items));
	}

	public CaseStatementSyntax AddCaseElseElseStatements(params StatementSyntax[] items)
	{
		CaseElseSyntax caseElseSyntax = CaseElse ?? SyntaxFactory.CaseElse();
		return WithCaseElse(caseElseSyntax.WithElseStatements(caseElseSyntax.ElseStatements.AddRange(items)));
	}
}
