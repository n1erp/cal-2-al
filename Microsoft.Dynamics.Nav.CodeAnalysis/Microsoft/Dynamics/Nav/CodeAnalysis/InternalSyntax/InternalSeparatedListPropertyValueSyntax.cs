using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalSeparatedListPropertyValueSyntax : InternalPropertyValueSyntax
{
	internal InternalSeparatedListPropertyValueSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalSeparatedListPropertyValueSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalSeparatedListPropertyValueSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
