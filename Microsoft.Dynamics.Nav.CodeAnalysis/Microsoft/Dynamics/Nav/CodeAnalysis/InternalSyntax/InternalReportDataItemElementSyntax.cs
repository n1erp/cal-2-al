using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalReportDataItemElementSyntax : InternalSyntaxNode
{
	internal InternalReportDataItemElementSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalReportDataItemElementSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalReportDataItemElementSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
