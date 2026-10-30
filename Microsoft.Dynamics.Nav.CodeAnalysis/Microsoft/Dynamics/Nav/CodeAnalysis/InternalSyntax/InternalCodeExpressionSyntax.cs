using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalCodeExpressionSyntax : InternalExpressionSyntax
{
	internal InternalCodeExpressionSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalCodeExpressionSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalCodeExpressionSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
