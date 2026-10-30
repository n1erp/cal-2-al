using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class QueryContinuationSyntax : CSharpSyntaxNode
{
	private QueryBodySyntax body;

	public SyntaxToken IntoKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.QueryContinuationSyntax)Green).intoKeyword, Position, 0);

	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.QueryContinuationSyntax)Green).identifier, GetChildPosition(1), GetChildIndex(1));

	public QueryBodySyntax Body => GetRed(ref body, 2);

	internal QueryContinuationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref body, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return body;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQueryContinuation(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitQueryContinuation(this);
	}

	public QueryContinuationSyntax Update(SyntaxToken intoKeyword, SyntaxToken identifier, QueryBodySyntax body)
	{
		if (intoKeyword != IntoKeyword || identifier != Identifier || body != Body)
		{
			QueryContinuationSyntax queryContinuationSyntax = SyntaxFactory.QueryContinuation(intoKeyword, identifier, body);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return queryContinuationSyntax.WithAnnotations(annotations);
			}
			return queryContinuationSyntax;
		}
		return this;
	}

	public QueryContinuationSyntax WithIntoKeyword(SyntaxToken intoKeyword)
	{
		return Update(intoKeyword, Identifier, Body);
	}

	public QueryContinuationSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(IntoKeyword, identifier, Body);
	}

	public QueryContinuationSyntax WithBody(QueryBodySyntax body)
	{
		return Update(IntoKeyword, Identifier, body);
	}

	public QueryContinuationSyntax AddBodyClauses(params QueryClauseSyntax[] items)
	{
		return WithBody(Body.WithClauses(Body.Clauses.AddRange(items)));
	}
}
