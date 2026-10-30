using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalReportLabelBaseSyntax : InternalSyntaxNode
{
	public abstract InternalIdentifierNameSyntax Name { get; }

	internal InternalReportLabelBaseSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalReportLabelBaseSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalReportLabelBaseSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
