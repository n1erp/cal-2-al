using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class QueryContinuationSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken intoKeyword;

	internal readonly SyntaxToken identifier;

	internal readonly QueryBodySyntax body;

	public SyntaxToken IntoKeyword => intoKeyword;

	public SyntaxToken Identifier => identifier;

	public QueryBodySyntax Body => body;

	internal QueryContinuationSyntax(SyntaxKind kind, SyntaxToken intoKeyword, SyntaxToken identifier, QueryBodySyntax body, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(intoKeyword);
		this.intoKeyword = intoKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal QueryContinuationSyntax(SyntaxKind kind, SyntaxToken intoKeyword, SyntaxToken identifier, QueryBodySyntax body, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(intoKeyword);
		this.intoKeyword = intoKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal QueryContinuationSyntax(SyntaxKind kind, SyntaxToken intoKeyword, SyntaxToken identifier, QueryBodySyntax body)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(intoKeyword);
		this.intoKeyword = intoKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(body);
		this.body = body;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)intoKeyword, 
			1 => identifier, 
			2 => body, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.QueryContinuationSyntax(this, parent, position);
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
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				queryContinuationSyntax = queryContinuationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				queryContinuationSyntax = queryContinuationSyntax.WithAnnotationsGreen(annotations);
			}
			return queryContinuationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new QueryContinuationSyntax(Kind, intoKeyword, identifier, body, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new QueryContinuationSyntax(Kind, intoKeyword, identifier, body, GetDiagnostics(), annotations);
	}

	internal QueryContinuationSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			intoKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			identifier = syntaxToken2;
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
		writer.WriteValue(intoKeyword);
		writer.WriteValue(identifier);
		writer.WriteValue(body);
	}

	static QueryContinuationSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(QueryContinuationSyntax), (ObjectReader r) => new QueryContinuationSyntax(r));
	}
}
