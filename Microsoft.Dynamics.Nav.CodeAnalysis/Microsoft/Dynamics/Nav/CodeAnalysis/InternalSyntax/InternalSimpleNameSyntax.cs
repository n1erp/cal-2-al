using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalSimpleNameSyntax : InternalNameSyntax
{
	public abstract InternalSyntaxToken Identifier { get; }

	internal InternalSimpleNameSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalSimpleNameSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalSimpleNameSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
