using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalControlChangeBaseSyntax : InternalSyntaxNode
{
	public abstract InternalSyntaxToken ChangeKeyword { get; }

	public abstract InternalSyntaxToken OpenParenthesisToken { get; }

	public abstract InternalSyntaxToken CloseParenthesisToken { get; }

	internal InternalControlChangeBaseSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalControlChangeBaseSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalControlChangeBaseSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}
}
