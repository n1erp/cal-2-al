using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class QueryElementsSyntax : SyntaxNode
{
	private SyntaxNode _dataItems;

	public SyntaxToken QueryElementsKeyword => new SyntaxToken(this, ((InternalQueryElementsSyntax)Node)._queryElementsKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalQueryElementsSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<QueryDataItemSyntax> DataItems => new SyntaxList<QueryDataItemSyntax>(GetRed(ref _dataItems, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalQueryElementsSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal QueryElementsSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _dataItems, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _dataItems;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQueryElements(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitQueryElements(this);
	}

	public QueryElementsSyntax Update(SyntaxToken queryElementsKeyword, SyntaxToken openBraceToken, SyntaxList<QueryDataItemSyntax> dataItems, SyntaxToken closeBraceToken)
	{
		if (queryElementsKeyword != QueryElementsKeyword || openBraceToken != OpenBraceToken || dataItems != DataItems || closeBraceToken != CloseBraceToken)
		{
			QueryElementsSyntax queryElementsSyntax = SyntaxFactory.QueryElements(queryElementsKeyword, openBraceToken, dataItems, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return queryElementsSyntax.WithAnnotations(annotations);
			}
			return queryElementsSyntax;
		}
		return this;
	}

	public QueryElementsSyntax WithQueryElementsKeyword(SyntaxToken queryElementsKeyword)
	{
		return Update(queryElementsKeyword, OpenBraceToken, DataItems, CloseBraceToken);
	}

	public QueryElementsSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(QueryElementsKeyword, openBraceToken, DataItems, CloseBraceToken);
	}

	public QueryElementsSyntax WithDataItems(SyntaxList<QueryDataItemSyntax> dataItems)
	{
		return Update(QueryElementsKeyword, OpenBraceToken, dataItems, CloseBraceToken);
	}

	public QueryElementsSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(QueryElementsKeyword, OpenBraceToken, DataItems, closeBraceToken);
	}

	public QueryElementsSyntax AddDataItems(params QueryDataItemSyntax[] items)
	{
		return WithDataItems(DataItems.AddRange(items));
	}
}
