using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalApplicationObjectExtensionSyntax : InternalApplicationObjectSyntax
{
	public abstract InternalObjectNameOrIdSyntax BaseObject { get; }

	internal InternalApplicationObjectExtensionSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalApplicationObjectExtensionSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalApplicationObjectExtensionSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
