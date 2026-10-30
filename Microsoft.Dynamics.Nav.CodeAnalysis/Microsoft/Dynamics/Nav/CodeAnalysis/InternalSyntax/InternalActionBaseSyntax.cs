using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalActionBaseSyntax : InternalSyntaxNode
{
	public abstract InternalSyntaxToken ActionKeyword { get; }

	public abstract InternalSyntaxToken OpenParenthesisToken { get; }

	public abstract InternalIdentifierNameSyntax Name { get; }

	public abstract InternalSyntaxToken CloseParenthesisToken { get; }

	public abstract InternalSyntaxToken OpenBraceToken { get; }

	public abstract InternalPropertyListSyntax PropertyList { get; }

	public abstract InternalSyntaxToken CloseBraceToken { get; }

	internal InternalActionBaseSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalActionBaseSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalActionBaseSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
