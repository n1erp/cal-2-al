namespace Microsoft.CodeAnalysis.CSharp;

internal struct RegionAnalysisContext
{
	public readonly CSharpCompilation Compilation;

	public readonly Symbol Member;

	public readonly BoundNode BoundNode;

	public readonly BoundNode FirstInRegion;

	public readonly BoundNode LastInRegion;

	public readonly bool Failed;

	public RegionAnalysisContext(CSharpCompilation compilation, Symbol member, BoundNode boundNode, BoundNode firstInRegion, BoundNode lastInRegion)
	{
		Compilation = compilation;
		Member = member;
		BoundNode = boundNode;
		FirstInRegion = firstInRegion;
		LastInRegion = lastInRegion;
		Failed = boundNode == null || firstInRegion == null || lastInRegion == null || firstInRegion.Syntax.SpanStart > lastInRegion.Syntax.Span.End;
		if (!Failed && firstInRegion == lastInRegion)
		{
			BoundKind kind = firstInRegion.Kind;
			if (kind == BoundKind.TypeExpression || kind == BoundKind.NamespaceExpression)
			{
				Failed = true;
			}
		}
	}
}
