using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal abstract class InternalStructuredTriviaSyntax : InternalSyntaxNode
{
	internal InternalStructuredTriviaSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics = null, SyntaxAnnotation[] annotations = null)
		: base(kind, diagnostics, annotations)
	{
		Initialize();
	}

	internal InternalStructuredTriviaSyntax(SyntaxKind kind, LexerMode lexerMode)
		: base(kind, lexerMode)
	{
		Initialize();
	}

	private void Initialize()
	{
		Flags |= NodeFlags.ContainsStructuredTrivia;
		if (Kind == SyntaxKind.SkippedTokensTrivia)
		{
			Flags |= NodeFlags.ContainsSkippedText;
		}
	}
}
