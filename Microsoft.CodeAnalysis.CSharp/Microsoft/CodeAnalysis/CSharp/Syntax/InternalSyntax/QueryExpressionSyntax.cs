using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class QueryExpressionSyntax : ExpressionSyntax
{
	internal readonly FromClauseSyntax fromClause;

	internal readonly QueryBodySyntax body;

	public FromClauseSyntax FromClause => fromClause;

	public QueryBodySyntax Body => body;

	internal QueryExpressionSyntax(SyntaxKind kind, FromClauseSyntax fromClause, QueryBodySyntax body, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(fromClause);
		this.fromClause = fromClause;
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal QueryExpressionSyntax(SyntaxKind kind, FromClauseSyntax fromClause, QueryBodySyntax body, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(fromClause);
		this.fromClause = fromClause;
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal QueryExpressionSyntax(SyntaxKind kind, FromClauseSyntax fromClause, QueryBodySyntax body)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(fromClause);
		this.fromClause = fromClause;
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)fromClause, 
			1 => body, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.QueryExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitQueryExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitQueryExpression(this);
	}

	public QueryExpressionSyntax Update(FromClauseSyntax fromClause, QueryBodySyntax body)
	{
		if (fromClause != FromClause || body != Body)
		{
			QueryExpressionSyntax queryExpressionSyntax = SyntaxFactory.QueryExpression(fromClause, body);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				queryExpressionSyntax = queryExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				queryExpressionSyntax = queryExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return queryExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new QueryExpressionSyntax(Kind, fromClause, body, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new QueryExpressionSyntax(Kind, fromClause, body, GetDiagnostics(), annotations);
	}

	internal QueryExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		FromClauseSyntax fromClauseSyntax = (FromClauseSyntax)reader.ReadValue();
		if (fromClauseSyntax != null)
		{
			AdjustFlagsAndWidth(fromClauseSyntax);
			fromClause = fromClauseSyntax;
		}
		QueryBodySyntax queryBodySyntax = (QueryBodySyntax)reader.ReadValue();
		if (queryBodySyntax != null)
		{
			AdjustFlagsAndWidth(queryBodySyntax);
			body = queryBodySyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(fromClause);
		writer.WriteValue(body);
	}

	static QueryExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(QueryExpressionSyntax), (ObjectReader r) => new QueryExpressionSyntax(r));
	}
}
