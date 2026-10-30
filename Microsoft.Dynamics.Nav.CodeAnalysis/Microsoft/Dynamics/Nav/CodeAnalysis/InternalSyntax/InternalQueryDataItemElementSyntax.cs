using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalQueryDataItemElementSyntax : InternalSyntaxNode
{
	internal InternalQueryDataItemElementSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalQueryDataItemElementSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalQueryDataItemElementSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
