using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalDataTypeSyntax : InternalSyntaxNode
{
	public abstract InternalSyntaxToken TypeName { get; }

	internal InternalDataTypeSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalDataTypeSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalDataTypeSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
