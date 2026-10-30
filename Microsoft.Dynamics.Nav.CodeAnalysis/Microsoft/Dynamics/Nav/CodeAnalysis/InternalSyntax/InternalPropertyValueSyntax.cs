using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalPropertyValueSyntax : InternalSyntaxNode
{
	internal InternalPropertyValueSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalPropertyValueSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalPropertyValueSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
