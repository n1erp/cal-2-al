using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalFieldGroupChangeBaseSyntax : InternalSyntaxNode
{
	public abstract InternalSyntaxToken OpenParenthesisToken { get; }

	public abstract InternalIdentifierNameSyntax Anchor { get; }

	public abstract InternalSyntaxToken CloseParenthesisToken { get; }

	public abstract InternalSyntaxToken OpenBraceToken { get; }

	public abstract InternalPropertyListSyntax PropertyList { get; }

	public abstract InternalSyntaxToken CloseBraceToken { get; }

	internal InternalFieldGroupChangeBaseSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalFieldGroupChangeBaseSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalFieldGroupChangeBaseSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
