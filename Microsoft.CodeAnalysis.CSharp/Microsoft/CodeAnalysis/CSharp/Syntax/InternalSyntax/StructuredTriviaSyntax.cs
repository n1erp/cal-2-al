using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class StructuredTriviaSyntax : CSharpSyntaxNode
{
	internal StructuredTriviaSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics = null, SyntaxAnnotation[] annotations = null)
		: base(kind, diagnostics, annotations)
	{
		Initialize();
	}

	internal StructuredTriviaSyntax(ObjectReader reader)
		: base(reader)
	{
		Initialize();
	}

	private void Initialize()
	{
		flags |= NodeFlags.ContainsStructuredTrivia;
		if (Kind == SyntaxKind.SkippedTokensTrivia)
		{
			flags |= NodeFlags.ContainsSkippedText;
		}
	}
}
