using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalExpressionSyntax : InternalSyntaxNode
{
	internal InternalExpressionSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalExpressionSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalExpressionSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
