using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalPropertyExpressionSyntax : InternalExpressionSyntax
{
	internal InternalPropertyExpressionSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalPropertyExpressionSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalPropertyExpressionSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
