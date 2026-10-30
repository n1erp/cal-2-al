using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalApplicationObjectSyntax : InternalObjectSyntax
{
	public abstract InternalObjectIdSyntax ObjectId { get; }

	internal InternalApplicationObjectSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalApplicationObjectSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalApplicationObjectSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
