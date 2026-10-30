using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class QueryClauseSyntax : CSharpSyntaxNode
{
	internal QueryClauseSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal QueryClauseSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected QueryClauseSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
