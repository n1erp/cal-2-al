using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalQueryDataItemSyntax : InternalQueryDataItemElementSyntax
{
	internal readonly InternalSyntaxToken _dataItemKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal readonly InternalObjectNameOrIdSyntax _dataItemTable;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxNode _elements;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => QueryDataItemSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken DataItemKeyword => _dataItemKeyword;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	public InternalObjectNameOrIdSyntax DataItemTable => _dataItemTable;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalPropertyListSyntax PropertyList => _propertyList;

	public InternalSyntaxList<InternalQueryDataItemElementSyntax> Elements => new InternalSyntaxList<InternalQueryDataItemElementSyntax>(_elements);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalQueryDataItemSyntax(SyntaxKind kind, InternalSyntaxToken dataItemKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax dataItemTable, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode elements, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(dataItemKeyword);
		_dataItemKeyword = dataItemKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(dataItemTable);
		_dataItemTable = dataItemTable;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (elements != null)
		{
			AdjustFlagsAndWidth(elements);
			_elements = elements;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalQueryDataItemSyntax(SyntaxKind kind, InternalSyntaxToken dataItemKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax dataItemTable, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode elements, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(dataItemKeyword);
		_dataItemKeyword = dataItemKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(dataItemTable);
		_dataItemTable = dataItemTable;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (elements != null)
		{
			AdjustFlagsAndWidth(elements);
			_elements = elements;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalQueryDataItemSyntax(SyntaxKind kind, InternalSyntaxToken dataItemKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax dataItemTable, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode elements, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(dataItemKeyword);
		_dataItemKeyword = dataItemKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(dataItemTable);
		_dataItemTable = dataItemTable;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (elements != null)
		{
			AdjustFlagsAndWidth(elements);
			_elements = elements;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _dataItemKeyword, 
			1 => _openParenthesisToken, 
			2 => _name, 
			3 => _semicolonToken, 
			4 => _dataItemTable, 
			5 => _closeParenthesisToken, 
			6 => _openBraceToken, 
			7 => _propertyList, 
			8 => _elements, 
			9 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new QueryDataItemSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQueryDataItem(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitQueryDataItem(this);
	}

	public InternalQueryDataItemSyntax Update(InternalSyntaxToken dataItemKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalObjectNameOrIdSyntax dataItemTable, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalQueryDataItemElementSyntax> elements, InternalSyntaxToken closeBraceToken)
	{
		if (dataItemKeyword != DataItemKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || dataItemTable != DataItemTable || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || elements != Elements || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.QueryDataItem(dataItemKeyword, openParenthesisToken, name, semicolonToken, dataItemTable, closeParenthesisToken, openBraceToken, propertyList, elements, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalQueryDataItemSyntax(Kind, _dataItemKeyword, _openParenthesisToken, _name, _semicolonToken, _dataItemTable, _closeParenthesisToken, _openBraceToken, _propertyList, _elements, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalQueryDataItemSyntax(Kind, _dataItemKeyword, _openParenthesisToken, _name, _semicolonToken, _dataItemTable, _closeParenthesisToken, _openBraceToken, _propertyList, _elements, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
