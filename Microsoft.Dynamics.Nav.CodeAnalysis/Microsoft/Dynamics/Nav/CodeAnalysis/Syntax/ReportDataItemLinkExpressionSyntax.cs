using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ReportDataItemLinkExpressionSyntax : PropertyExpressionSyntax
{
	private IdentifierNameSyntax _sourceField;

	private IdentifierNameSyntax _relatedField;

	public IdentifierNameSyntax SourceField => GetRedAtZero(ref _sourceField);

	public SyntaxToken EqualsToken => new SyntaxToken(this, ((InternalReportDataItemLinkExpressionSyntax)Node)._equalsToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken FieldKeywordToken => new SyntaxToken(this, ((InternalReportDataItemLinkExpressionSyntax)Node)._fieldKeywordToken, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalReportDataItemLinkExpressionSyntax)Node)._openParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public IdentifierNameSyntax RelatedField => GetRed(ref _relatedField, 4);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalReportDataItemLinkExpressionSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	internal ReportDataItemLinkExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _sourceField), 
			4 => GetRed(ref _relatedField, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_sourceField, 
			4 => _relatedField, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReportDataItemLinkExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitReportDataItemLinkExpression(this);
	}

	public ReportDataItemLinkExpressionSyntax Update(IdentifierNameSyntax sourceField, SyntaxToken equalsToken, SyntaxToken fieldKeywordToken, SyntaxToken openParenthesisToken, IdentifierNameSyntax relatedField, SyntaxToken closeParenthesisToken)
	{
		if (sourceField != SourceField || equalsToken != EqualsToken || fieldKeywordToken != FieldKeywordToken || openParenthesisToken != OpenParenthesisToken || relatedField != RelatedField || closeParenthesisToken != CloseParenthesisToken)
		{
			ReportDataItemLinkExpressionSyntax reportDataItemLinkExpressionSyntax = SyntaxFactory.ReportDataItemLinkExpression(sourceField, equalsToken, fieldKeywordToken, openParenthesisToken, relatedField, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return reportDataItemLinkExpressionSyntax.WithAnnotations(annotations);
			}
			return reportDataItemLinkExpressionSyntax;
		}
		return this;
	}

	public ReportDataItemLinkExpressionSyntax WithSourceField(IdentifierNameSyntax sourceField)
	{
		return Update(sourceField, EqualsToken, FieldKeywordToken, OpenParenthesisToken, RelatedField, CloseParenthesisToken);
	}

	public ReportDataItemLinkExpressionSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(SourceField, equalsToken, FieldKeywordToken, OpenParenthesisToken, RelatedField, CloseParenthesisToken);
	}

	public ReportDataItemLinkExpressionSyntax WithFieldKeywordToken(SyntaxToken fieldKeywordToken)
	{
		return Update(SourceField, EqualsToken, fieldKeywordToken, OpenParenthesisToken, RelatedField, CloseParenthesisToken);
	}

	public ReportDataItemLinkExpressionSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(SourceField, EqualsToken, FieldKeywordToken, openParenthesisToken, RelatedField, CloseParenthesisToken);
	}

	public ReportDataItemLinkExpressionSyntax WithRelatedField(IdentifierNameSyntax relatedField)
	{
		return Update(SourceField, EqualsToken, FieldKeywordToken, OpenParenthesisToken, relatedField, CloseParenthesisToken);
	}

	public ReportDataItemLinkExpressionSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(SourceField, EqualsToken, FieldKeywordToken, OpenParenthesisToken, RelatedField, closeParenthesisToken);
	}
}
