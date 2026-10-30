using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalCompilationUnitSyntax : InternalSyntaxNode
{
	internal InternalCompilationUnitSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalCompilationUnitSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalCompilationUnitSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
