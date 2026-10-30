using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalQueryElementSyntax : InternalQueryDataItemElementSyntax
{
	public abstract InternalSyntaxToken OpenParenthesisToken { get; }

	public abstract InternalIdentifierNameSyntax Name { get; }

	public abstract InternalSyntaxToken SemicolonToken { get; }

	public abstract InternalIdentifierNameSyntax RelatedField { get; }

	public abstract InternalSyntaxToken CloseParenthesisToken { get; }

	public abstract InternalSyntaxToken OpenBraceToken { get; }

	public abstract InternalPropertyListSyntax PropertyList { get; }

	public abstract InternalSyntaxToken CloseBraceToken { get; }

	internal InternalQueryElementSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalQueryElementSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalQueryElementSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
