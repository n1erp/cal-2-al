using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalLiteralValueSyntax : InternalSyntaxNode
{
	internal InternalLiteralValueSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalLiteralValueSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalLiteralValueSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
