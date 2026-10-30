using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class CrefSyntax : CSharpSyntaxNode
{
	internal CrefSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal CrefSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected CrefSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
