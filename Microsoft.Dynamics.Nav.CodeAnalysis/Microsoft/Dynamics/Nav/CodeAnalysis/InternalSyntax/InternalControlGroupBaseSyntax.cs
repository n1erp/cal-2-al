using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalControlGroupBaseSyntax : InternalControlBaseSyntax
{
	public abstract InternalSyntaxList<InternalControlBaseSyntax> Controls { get; }

	internal InternalControlGroupBaseSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalControlGroupBaseSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalControlGroupBaseSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
