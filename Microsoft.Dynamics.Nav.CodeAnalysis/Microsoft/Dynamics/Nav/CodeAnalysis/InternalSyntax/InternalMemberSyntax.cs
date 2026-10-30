using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalMemberSyntax : InternalSyntaxNode
{
	internal InternalMemberSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalMemberSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalMemberSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
