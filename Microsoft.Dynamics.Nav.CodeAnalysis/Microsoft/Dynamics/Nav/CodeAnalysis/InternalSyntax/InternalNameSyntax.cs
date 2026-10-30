using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalNameSyntax : InternalCodeExpressionSyntax
{
	internal InternalNameSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalNameSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalNameSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
