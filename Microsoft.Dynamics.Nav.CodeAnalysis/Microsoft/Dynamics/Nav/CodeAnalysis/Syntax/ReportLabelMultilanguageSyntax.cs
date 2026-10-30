using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ReportLabelMultilanguageSyntax : ReportLabelBaseSyntax
{
	private IdentifierNameSyntax _name;

	private CommaSeparatedIdentifierEqualsStringListSyntax _caption;

	public SyntaxToken LabelKeyword => new SyntaxToken(this, ((InternalReportLabelMultilanguageSyntax)Node)._labelKeyword, Position, 0);

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalReportLabelMultilanguageSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalReportLabelMultilanguageSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	public CommaSeparatedIdentifierEqualsStringListSyntax Caption => GetRed(ref _caption, 4);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalReportLabelMultilanguageSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	internal ReportLabelMultilanguageSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref _name, 2), 
			4 => GetRed(ref _caption, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)_name, 
			4 => _caption, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReportLabelMultilanguage(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitReportLabelMultilanguage(this);
	}

	public ReportLabelMultilanguageSyntax Update(SyntaxToken labelKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, CommaSeparatedIdentifierEqualsStringListSyntax caption, SyntaxToken closeParenthesisToken)
	{
		if (labelKeyword != LabelKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || caption != Caption || closeParenthesisToken != CloseParenthesisToken)
		{
			ReportLabelMultilanguageSyntax reportLabelMultilanguageSyntax = SyntaxFactory.ReportLabelMultilanguage(labelKeyword, openParenthesisToken, name, semicolonToken, caption, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return reportLabelMultilanguageSyntax.WithAnnotations(annotations);
			}
			return reportLabelMultilanguageSyntax;
		}
		return this;
	}

	public ReportLabelMultilanguageSyntax WithLabelKeyword(SyntaxToken labelKeyword)
	{
		return Update(labelKeyword, OpenParenthesisToken, Name, SemicolonToken, Caption, CloseParenthesisToken);
	}

	public ReportLabelMultilanguageSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(LabelKeyword, openParenthesisToken, Name, SemicolonToken, Caption, CloseParenthesisToken);
	}

	public ReportLabelMultilanguageSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(LabelKeyword, OpenParenthesisToken, name, SemicolonToken, Caption, CloseParenthesisToken);
	}

	public ReportLabelMultilanguageSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(LabelKeyword, OpenParenthesisToken, Name, semicolonToken, Caption, CloseParenthesisToken);
	}

	public ReportLabelMultilanguageSyntax WithCaption(CommaSeparatedIdentifierEqualsStringListSyntax caption)
	{
		return Update(LabelKeyword, OpenParenthesisToken, Name, SemicolonToken, caption, CloseParenthesisToken);
	}

	public ReportLabelMultilanguageSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(LabelKeyword, OpenParenthesisToken, Name, SemicolonToken, Caption, closeParenthesisToken);
	}

	public ReportLabelMultilanguageSyntax AddCaptionValues(params IdentifierEqualsStringSyntax[] items)
	{
		return WithCaption(Caption.WithValues(Caption.Values.AddRange(items)));
	}
}
