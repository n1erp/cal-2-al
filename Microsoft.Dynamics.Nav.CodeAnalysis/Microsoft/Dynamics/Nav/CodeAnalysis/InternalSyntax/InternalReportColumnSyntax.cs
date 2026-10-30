using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalReportColumnSyntax : InternalReportDataItemElementSyntax
{
	internal readonly InternalSyntaxToken _columnKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal readonly InternalCodeExpressionSyntax _sourceExpression;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => ReportColumnSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ColumnKeyword => _columnKeyword;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	public InternalCodeExpressionSyntax SourceExpression => _sourceExpression;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalPropertyListSyntax PropertyList => _propertyList;

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalReportColumnSyntax(SyntaxKind kind, InternalSyntaxToken columnKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax sourceExpression, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(columnKeyword);
		_columnKeyword = columnKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(sourceExpression);
		_sourceExpression = sourceExpression;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalReportColumnSyntax(SyntaxKind kind, InternalSyntaxToken columnKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax sourceExpression, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(columnKeyword);
		_columnKeyword = columnKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(sourceExpression);
		_sourceExpression = sourceExpression;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalReportColumnSyntax(SyntaxKind kind, InternalSyntaxToken columnKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax sourceExpression, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(columnKeyword);
		_columnKeyword = columnKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(sourceExpression);
		_sourceExpression = sourceExpression;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_columnKeyword, 
			1 => _openParenthesisToken, 
			2 => _name, 
			3 => _semicolonToken, 
			4 => _sourceExpression, 
			5 => _closeParenthesisToken, 
			6 => _openBraceToken, 
			7 => _propertyList, 
			8 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ReportColumnSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReportColumn(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitReportColumn(this);
	}

	public InternalReportColumnSyntax Update(InternalSyntaxToken columnKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCodeExpressionSyntax sourceExpression, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxToken closeBraceToken)
	{
		if (columnKeyword != ColumnKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || sourceExpression != SourceExpression || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.ReportColumn(columnKeyword, openParenthesisToken, name, semicolonToken, sourceExpression, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalReportColumnSyntax(Kind, _columnKeyword, _openParenthesisToken, _name, _semicolonToken, _sourceExpression, _closeParenthesisToken, _openBraceToken, _propertyList, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalReportColumnSyntax(Kind, _columnKeyword, _openParenthesisToken, _name, _semicolonToken, _sourceExpression, _closeParenthesisToken, _openBraceToken, _propertyList, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
