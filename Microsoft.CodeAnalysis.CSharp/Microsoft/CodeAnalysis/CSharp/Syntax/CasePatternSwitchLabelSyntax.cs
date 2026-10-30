using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class CasePatternSwitchLabelSyntax : SwitchLabelSyntax
{
	private PatternSyntax pattern;

	private WhenClauseSyntax whenClause;

	public override SyntaxToken Keyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CasePatternSwitchLabelSyntax)Green).keyword, Position, 0);

	public PatternSyntax Pattern => GetRed(ref pattern, 1);

	public WhenClauseSyntax WhenClause => GetRed(ref whenClause, 2);

	public override SyntaxToken ColonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CasePatternSwitchLabelSyntax)Green).colonToken, GetChildPosition(3), GetChildIndex(3));

	internal CasePatternSwitchLabelSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)GetRed(ref pattern, 1), 
			2 => GetRed(ref whenClause, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)pattern, 
			2 => whenClause, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCasePatternSwitchLabel(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCasePatternSwitchLabel(this);
	}

	public CasePatternSwitchLabelSyntax Update(SyntaxToken keyword, PatternSyntax pattern, WhenClauseSyntax whenClause, SyntaxToken colonToken)
	{
		if (keyword != Keyword || pattern != Pattern || whenClause != WhenClause || colonToken != ColonToken)
		{
			CasePatternSwitchLabelSyntax casePatternSwitchLabelSyntax = SyntaxFactory.CasePatternSwitchLabel(keyword, pattern, whenClause, colonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return casePatternSwitchLabelSyntax.WithAnnotations(annotations);
			}
			return casePatternSwitchLabelSyntax;
		}
		return this;
	}

	internal override SwitchLabelSyntax WithKeywordCore(SyntaxToken keyword)
	{
		return WithKeyword(keyword);
	}

	public new CasePatternSwitchLabelSyntax WithKeyword(SyntaxToken keyword)
	{
		return Update(keyword, Pattern, WhenClause, ColonToken);
	}

	public CasePatternSwitchLabelSyntax WithPattern(PatternSyntax pattern)
	{
		return Update(Keyword, pattern, WhenClause, ColonToken);
	}

	public CasePatternSwitchLabelSyntax WithWhenClause(WhenClauseSyntax whenClause)
	{
		return Update(Keyword, Pattern, whenClause, ColonToken);
	}

	internal override SwitchLabelSyntax WithColonTokenCore(SyntaxToken colonToken)
	{
		return WithColonToken(colonToken);
	}

	public new CasePatternSwitchLabelSyntax WithColonToken(SyntaxToken colonToken)
	{
		return Update(Keyword, Pattern, WhenClause, colonToken);
	}
}
