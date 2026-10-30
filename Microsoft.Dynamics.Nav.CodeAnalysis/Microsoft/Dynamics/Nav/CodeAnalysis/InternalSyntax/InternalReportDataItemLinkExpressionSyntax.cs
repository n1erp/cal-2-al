using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalReportDataItemLinkExpressionSyntax : InternalPropertyExpressionSyntax
{
	internal readonly InternalIdentifierNameSyntax _sourceField;

	internal readonly InternalSyntaxToken _equalsToken;

	internal readonly InternalSyntaxToken _fieldKeywordToken;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _relatedField;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => ReportDataItemLinkExpressionSyntaxDefinition.Instance.Value;

	public InternalIdentifierNameSyntax SourceField => _sourceField;

	public InternalSyntaxToken EqualsToken => _equalsToken;

	public InternalSyntaxToken FieldKeywordToken => _fieldKeywordToken;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalIdentifierNameSyntax RelatedField => _relatedField;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalReportDataItemLinkExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax sourceField, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax relatedField, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(sourceField);
		_sourceField = sourceField;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(fieldKeywordToken);
		_fieldKeywordToken = fieldKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(relatedField);
		_relatedField = relatedField;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalReportDataItemLinkExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax sourceField, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax relatedField, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(sourceField);
		_sourceField = sourceField;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(fieldKeywordToken);
		_fieldKeywordToken = fieldKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(relatedField);
		_relatedField = relatedField;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalReportDataItemLinkExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax sourceField, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax relatedField, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(sourceField);
		_sourceField = sourceField;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		AdjustFlagsAndWidth(fieldKeywordToken);
		_fieldKeywordToken = fieldKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(relatedField);
		_relatedField = relatedField;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_sourceField, 
			1 => _equalsToken, 
			2 => _fieldKeywordToken, 
			3 => _openParenthesisToken, 
			4 => _relatedField, 
			5 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ReportDataItemLinkExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReportDataItemLinkExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitReportDataItemLinkExpression(this);
	}

	public InternalReportDataItemLinkExpressionSyntax Update(InternalIdentifierNameSyntax sourceField, InternalSyntaxToken equalsToken, InternalSyntaxToken fieldKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax relatedField, InternalSyntaxToken closeParenthesisToken)
	{
		if (sourceField != SourceField || equalsToken != EqualsToken || fieldKeywordToken != FieldKeywordToken || openParenthesisToken != OpenParenthesisToken || relatedField != RelatedField || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.ReportDataItemLinkExpression(sourceField, equalsToken, fieldKeywordToken, openParenthesisToken, relatedField, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalReportDataItemLinkExpressionSyntax(Kind, _sourceField, _equalsToken, _fieldKeywordToken, _openParenthesisToken, _relatedField, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalReportDataItemLinkExpressionSyntax(Kind, _sourceField, _equalsToken, _fieldKeywordToken, _openParenthesisToken, _relatedField, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
