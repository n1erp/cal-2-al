using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalControlBaseSyntax : InternalSyntaxNode
{
	public abstract InternalSyntaxToken ControlKeyword { get; }

	public abstract InternalSyntaxToken OpenParenthesisToken { get; }

	public abstract InternalIdentifierNameSyntax Name { get; }

	public abstract InternalSyntaxToken CloseParenthesisToken { get; }

	public abstract InternalSyntaxToken OpenBraceToken { get; }

	public abstract InternalPropertyListSyntax PropertyList { get; }

	public abstract InternalSyntaxToken CloseBraceToken { get; }

	internal InternalControlBaseSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalControlBaseSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalControlBaseSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
