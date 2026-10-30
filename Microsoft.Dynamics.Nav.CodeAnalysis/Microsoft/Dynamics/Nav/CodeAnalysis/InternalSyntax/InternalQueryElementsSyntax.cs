using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalQueryElementsSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _queryElementsKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _dataItems;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => QueryElementsSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken QueryElementsKeyword => _queryElementsKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalQueryDataItemSyntax> DataItems => new InternalSyntaxList<InternalQueryDataItemSyntax>(_dataItems);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalQueryElementsSyntax(SyntaxKind kind, InternalSyntaxToken queryElementsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode dataItems, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(queryElementsKeyword);
		_queryElementsKeyword = queryElementsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (dataItems != null)
		{
			AdjustFlagsAndWidth(dataItems);
			_dataItems = dataItems;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalQueryElementsSyntax(SyntaxKind kind, InternalSyntaxToken queryElementsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode dataItems, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(queryElementsKeyword);
		_queryElementsKeyword = queryElementsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (dataItems != null)
		{
			AdjustFlagsAndWidth(dataItems);
			_dataItems = dataItems;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalQueryElementsSyntax(SyntaxKind kind, InternalSyntaxToken queryElementsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode dataItems, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(queryElementsKeyword);
		_queryElementsKeyword = queryElementsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (dataItems != null)
		{
			AdjustFlagsAndWidth(dataItems);
			_dataItems = dataItems;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _queryElementsKeyword, 
			1 => _openBraceToken, 
			2 => _dataItems, 
			3 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new QueryElementsSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQueryElements(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitQueryElements(this);
	}

	public InternalQueryElementsSyntax Update(InternalSyntaxToken queryElementsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalQueryDataItemSyntax> dataItems, InternalSyntaxToken closeBraceToken)
	{
		if (queryElementsKeyword != QueryElementsKeyword || openBraceToken != OpenBraceToken || dataItems != DataItems || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.QueryElements(queryElementsKeyword, openBraceToken, dataItems, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalQueryElementsSyntax(Kind, _queryElementsKeyword, _openBraceToken, _dataItems, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalQueryElementsSyntax(Kind, _queryElementsKeyword, _openBraceToken, _dataItems, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
