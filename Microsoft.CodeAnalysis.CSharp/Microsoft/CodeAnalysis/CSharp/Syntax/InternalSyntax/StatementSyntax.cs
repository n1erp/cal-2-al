using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class StatementSyntax : CSharpSyntaxNode
{
	internal StatementSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal StatementSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected StatementSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
