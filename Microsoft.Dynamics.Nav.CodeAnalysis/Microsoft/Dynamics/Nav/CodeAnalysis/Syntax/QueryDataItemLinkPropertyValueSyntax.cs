using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class QueryDataItemLinkPropertyValueSyntax : PropertyValueSyntax
{
	private SyntaxNode _joins;

	public SeparatedSyntaxList<QueryDataItemLinkExpressionSyntax> Joins
	{
		get
		{
			SyntaxNode red = GetRed(ref _joins, 0);
			if (red != null)
			{
				return new SeparatedSyntaxList<QueryDataItemLinkExpressionSyntax>(red, 0);
			}
			return default;
		}
	}

	internal QueryDataItemLinkPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _joins);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _joins;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQueryDataItemLinkPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitQueryDataItemLinkPropertyValue(this);
	}

	public QueryDataItemLinkPropertyValueSyntax Update(SeparatedSyntaxList<QueryDataItemLinkExpressionSyntax> joins)
	{
		if (joins != Joins)
		{
			QueryDataItemLinkPropertyValueSyntax queryDataItemLinkPropertyValueSyntax = SyntaxFactory.QueryDataItemLinkPropertyValue(joins);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return queryDataItemLinkPropertyValueSyntax.WithAnnotations(annotations);
			}
			return queryDataItemLinkPropertyValueSyntax;
		}
		return this;
	}

	public QueryDataItemLinkPropertyValueSyntax WithJoins(SeparatedSyntaxList<QueryDataItemLinkExpressionSyntax> joins)
	{
		return Update(joins);
	}

	public QueryDataItemLinkPropertyValueSyntax AddJoins(params QueryDataItemLinkExpressionSyntax[] items)
	{
		return Update(Joins.AddRange(items));
	}
}
