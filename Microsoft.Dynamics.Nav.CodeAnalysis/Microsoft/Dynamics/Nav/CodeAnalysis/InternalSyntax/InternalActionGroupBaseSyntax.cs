using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalActionGroupBaseSyntax : InternalActionBaseSyntax
{
	public abstract InternalSyntaxList<InternalActionBaseSyntax> Actions { get; }

	internal InternalActionGroupBaseSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalActionGroupBaseSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalActionGroupBaseSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
