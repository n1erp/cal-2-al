using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class QueryDataItemLinkExpressionSyntax : ExpressionSyntax
{
	private IdentifierNameSyntax _sourceField;

	private MemberAccessExpressionSyntax _relatedField;

	public IdentifierNameSyntax SourceField => GetRedAtZero(ref _sourceField);

	public SyntaxToken EqualsToken => new SyntaxToken(this, ((InternalQueryDataItemLinkExpressionSyntax)Node)._equalsToken, GetChildPosition(1), GetChildIndex(1));

	public MemberAccessExpressionSyntax RelatedField => GetRed(ref _relatedField, 2);

	internal QueryDataItemLinkExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _sourceField), 
			2 => GetRed(ref _relatedField, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_sourceField, 
			2 => _relatedField, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQueryDataItemLinkExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitQueryDataItemLinkExpression(this);
	}

	public QueryDataItemLinkExpressionSyntax Update(IdentifierNameSyntax sourceField, SyntaxToken equalsToken, MemberAccessExpressionSyntax relatedField)
	{
		if (sourceField != SourceField || equalsToken != EqualsToken || relatedField != RelatedField)
		{
			QueryDataItemLinkExpressionSyntax queryDataItemLinkExpressionSyntax = SyntaxFactory.QueryDataItemLinkExpression(sourceField, equalsToken, relatedField);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return queryDataItemLinkExpressionSyntax.WithAnnotations(annotations);
			}
			return queryDataItemLinkExpressionSyntax;
		}
		return this;
	}

	public QueryDataItemLinkExpressionSyntax WithSourceField(IdentifierNameSyntax sourceField)
	{
		return Update(sourceField, EqualsToken, RelatedField);
	}

	public QueryDataItemLinkExpressionSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(SourceField, equalsToken, RelatedField);
	}

	public QueryDataItemLinkExpressionSyntax WithRelatedField(MemberAccessExpressionSyntax relatedField)
	{
		return Update(SourceField, EqualsToken, relatedField);
	}
}
