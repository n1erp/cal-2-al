using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalObjectSyntax : InternalSyntaxNode
{
	public abstract InternalIdentifierNameSyntax Name { get; }

	public abstract InternalPropertyListSyntax PropertyList { get; }

	public abstract InternalSyntaxList<InternalMemberSyntax> Members { get; }

	internal InternalObjectSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalObjectSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalObjectSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
