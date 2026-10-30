using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalKeyFieldGroupBaseSyntax : InternalSyntaxNode
{
	public abstract InternalSyntaxToken OpenParenthesisToken { get; }

	public abstract InternalSyntaxToken Id { get; }

	public abstract InternalSyntaxToken SemicolonToken1 { get; }

	public abstract InternalIdentifierNameSyntax Name { get; }

	public abstract InternalSyntaxToken SemicolonToken2 { get; }

	public abstract InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> Fields { get; }

	public abstract InternalSyntaxToken CloseParenthesisToken { get; }

	public abstract InternalSyntaxToken OpenBraceToken { get; }

	public abstract InternalPropertyListSyntax PropertyList { get; }

	public abstract InternalSyntaxToken CloseBraceToken { get; }

	internal InternalKeyFieldGroupBaseSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalKeyFieldGroupBaseSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalKeyFieldGroupBaseSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
