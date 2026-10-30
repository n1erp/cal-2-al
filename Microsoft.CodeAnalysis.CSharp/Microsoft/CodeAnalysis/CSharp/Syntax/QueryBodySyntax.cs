using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class QueryBodySyntax : CSharpSyntaxNode
{
	private SyntaxNode clauses;

	private SelectOrGroupClauseSyntax selectOrGroup;

	private QueryContinuationSyntax continuation;

	public SyntaxList<QueryClauseSyntax> Clauses => new SyntaxList<QueryClauseSyntax>(GetRed(ref clauses, 0));

	public SelectOrGroupClauseSyntax SelectOrGroup => GetRed(ref selectOrGroup, 1);

	public QueryContinuationSyntax Continuation => GetRed(ref continuation, 2);

	internal QueryBodySyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref clauses), 
			1 => GetRed(ref selectOrGroup, 1), 
			2 => GetRed(ref continuation, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => clauses, 
			1 => selectOrGroup, 
			2 => continuation, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQueryBody(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitQueryBody(this);
	}

	public QueryBodySyntax Update(SyntaxList<QueryClauseSyntax> clauses, SelectOrGroupClauseSyntax selectOrGroup, QueryContinuationSyntax continuation)
	{
		if (clauses != Clauses || selectOrGroup != SelectOrGroup || continuation != Continuation)
		{
			QueryBodySyntax queryBodySyntax = SyntaxFactory.QueryBody(clauses, selectOrGroup, continuation);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return queryBodySyntax.WithAnnotations(annotations);
			}
			return queryBodySyntax;
		}
		return this;
	}

	public QueryBodySyntax WithClauses(SyntaxList<QueryClauseSyntax> clauses)
	{
		return Update(clauses, SelectOrGroup, Continuation);
	}

	public QueryBodySyntax WithSelectOrGroup(SelectOrGroupClauseSyntax selectOrGroup)
	{
		return Update(Clauses, selectOrGroup, Continuation);
	}

	public QueryBodySyntax WithContinuation(QueryContinuationSyntax continuation)
	{
		return Update(Clauses, SelectOrGroup, continuation);
	}

	public QueryBodySyntax AddClauses(params QueryClauseSyntax[] items)
	{
		return WithClauses(Clauses.AddRange(items));
	}
}
