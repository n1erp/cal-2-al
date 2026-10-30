using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalAbstractPageExtensionSyntax : InternalApplicationObjectExtensionSyntax
{
	public abstract InternalSyntaxToken OpenBraceToken { get; }

	public abstract InternalPageExtensionLayoutSyntax Layout { get; }

	public abstract InternalPageExtensionActionListSyntax Actions { get; }

	public abstract InternalPageExtensionViewListSyntax Views { get; }

	public abstract InternalSyntaxToken CloseBraceToken { get; }

	internal InternalAbstractPageExtensionSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalAbstractPageExtensionSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalAbstractPageExtensionSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
