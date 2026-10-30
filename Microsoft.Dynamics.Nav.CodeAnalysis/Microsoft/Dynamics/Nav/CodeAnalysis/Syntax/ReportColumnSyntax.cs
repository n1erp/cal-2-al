using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ReportColumnSyntax : ReportDataItemElementSyntax
{
	private IdentifierNameSyntax _name;

	private CodeExpressionSyntax _sourceExpression;

	private PropertyListSyntax _propertyList;

	public SyntaxToken ColumnKeyword => new SyntaxToken(this, ((InternalReportColumnSyntax)Node)._columnKeyword, Position, 0);

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalReportColumnSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalReportColumnSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	public CodeExpressionSyntax SourceExpression => GetRed(ref _sourceExpression, 4);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalReportColumnSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalReportColumnSyntax)Node)._openBraceToken, GetChildPosition(6), GetChildIndex(6));

	public PropertyListSyntax PropertyList => GetRed(ref _propertyList, 7);

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalReportColumnSyntax)Node)._closeBraceToken, GetChildPosition(8), GetChildIndex(8));

	internal ReportColumnSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref _name, 2), 
			4 => GetRed(ref _sourceExpression, 4), 
			7 => GetRed(ref _propertyList, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)_name, 
			4 => _sourceExpression, 
			7 => _propertyList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReportColumn(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitReportColumn(this);
	}

	public ReportColumnSyntax Update(SyntaxToken columnKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, CodeExpressionSyntax sourceExpression, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		if (columnKeyword != ColumnKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || sourceExpression != SourceExpression || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			ReportColumnSyntax reportColumnSyntax = SyntaxFactory.ReportColumn(columnKeyword, openParenthesisToken, name, semicolonToken, sourceExpression, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return reportColumnSyntax.WithAnnotations(annotations);
			}
			return reportColumnSyntax;
		}
		return this;
	}

	public ReportColumnSyntax WithColumnKeyword(SyntaxToken columnKeyword)
	{
		return Update(columnKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceExpression, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public ReportColumnSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ColumnKeyword, openParenthesisToken, Name, SemicolonToken, SourceExpression, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public ReportColumnSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ColumnKeyword, OpenParenthesisToken, name, SemicolonToken, SourceExpression, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public ReportColumnSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(ColumnKeyword, OpenParenthesisToken, Name, semicolonToken, SourceExpression, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public ReportColumnSyntax WithSourceExpression(CodeExpressionSyntax sourceExpression)
	{
		return Update(ColumnKeyword, OpenParenthesisToken, Name, SemicolonToken, sourceExpression, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public ReportColumnSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ColumnKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceExpression, closeParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public ReportColumnSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ColumnKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceExpression, CloseParenthesisToken, openBraceToken, PropertyList, CloseBraceToken);
	}

	public ReportColumnSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ColumnKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceExpression, CloseParenthesisToken, OpenBraceToken, propertyList, CloseBraceToken);
	}

	public ReportColumnSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ColumnKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceExpression, CloseParenthesisToken, OpenBraceToken, PropertyList, closeBraceToken);
	}

	public ReportColumnSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}
}
