using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalXmlPortFieldNodeSyntax : InternalXmlPortNodeSyntax
{
	public abstract InternalSyntaxToken SemicolonToken { get; }

	public abstract InternalCodeExpressionSyntax SourceField { get; }

	internal InternalXmlPortFieldNodeSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalXmlPortFieldNodeSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalXmlPortFieldNodeSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
