using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalAttributeArgumentSyntax : InternalSyntaxNode
{
	internal InternalAttributeArgumentSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalAttributeArgumentSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalAttributeArgumentSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
