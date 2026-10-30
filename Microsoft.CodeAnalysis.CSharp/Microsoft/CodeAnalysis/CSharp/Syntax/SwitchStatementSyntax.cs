using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class SwitchStatementSyntax : StatementSyntax
{
	private ExpressionSyntax expression;

	private SyntaxNode sections;

	public SyntaxToken SwitchKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SwitchStatementSyntax)Green).switchKeyword, Position, 0);

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SwitchStatementSyntax)Green).openParenToken, GetChildPosition(1), GetChildIndex(1));

	public ExpressionSyntax Expression => GetRed(ref expression, 2);

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SwitchStatementSyntax)Green).closeParenToken, GetChildPosition(3), GetChildIndex(3));

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SwitchStatementSyntax)Green).openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public SyntaxList<SwitchSectionSyntax> Sections => new SyntaxList<SwitchSectionSyntax>(GetRed(ref sections, 5));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SwitchStatementSyntax)Green).closeBraceToken, GetChildPosition(6), GetChildIndex(6));

	internal SwitchStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref expression, 2), 
			5 => GetRed(ref sections, 5), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => expression, 
			5 => sections, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSwitchStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSwitchStatement(this);
	}

	public SwitchStatementSyntax Update(SyntaxToken switchKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, SyntaxToken openBraceToken, SyntaxList<SwitchSectionSyntax> sections, SyntaxToken closeBraceToken)
	{
		if (switchKeyword != SwitchKeyword || openParenToken != OpenParenToken || expression != Expression || closeParenToken != CloseParenToken || openBraceToken != OpenBraceToken || sections != Sections || closeBraceToken != CloseBraceToken)
		{
			SwitchStatementSyntax switchStatementSyntax = SyntaxFactory.SwitchStatement(switchKeyword, openParenToken, expression, closeParenToken, openBraceToken, sections, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return switchStatementSyntax.WithAnnotations(annotations);
			}
			return switchStatementSyntax;
		}
		return this;
	}

	public SwitchStatementSyntax WithSwitchKeyword(SyntaxToken switchKeyword)
	{
		return Update(switchKeyword, OpenParenToken, Expression, CloseParenToken, OpenBraceToken, Sections, CloseBraceToken);
	}

	public SwitchStatementSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(SwitchKeyword, openParenToken, Expression, CloseParenToken, OpenBraceToken, Sections, CloseBraceToken);
	}

	public SwitchStatementSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(SwitchKeyword, OpenParenToken, expression, CloseParenToken, OpenBraceToken, Sections, CloseBraceToken);
	}

	public SwitchStatementSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(SwitchKeyword, OpenParenToken, Expression, closeParenToken, OpenBraceToken, Sections, CloseBraceToken);
	}

	public SwitchStatementSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(SwitchKeyword, OpenParenToken, Expression, CloseParenToken, openBraceToken, Sections, CloseBraceToken);
	}

	public SwitchStatementSyntax WithSections(SyntaxList<SwitchSectionSyntax> sections)
	{
		return Update(SwitchKeyword, OpenParenToken, Expression, CloseParenToken, OpenBraceToken, sections, CloseBraceToken);
	}

	public SwitchStatementSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(SwitchKeyword, OpenParenToken, Expression, CloseParenToken, OpenBraceToken, Sections, closeBraceToken);
	}

	public SwitchStatementSyntax AddSections(params SwitchSectionSyntax[] items)
	{
		return WithSections(Sections.AddRange(items));
	}
}
