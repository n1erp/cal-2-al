using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalXmlPortTextNodeSyntax : InternalXmlPortNodeSyntax
{
	internal InternalXmlPortTextNodeSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalXmlPortTextNodeSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalXmlPortTextNodeSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
