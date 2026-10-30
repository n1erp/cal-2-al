using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class InternalStatementSyntax : InternalSyntaxNode
{
	internal bool HasSemicolon => GetLastTerminal().Kind == SyntaxKind.SemicolonToken;

	internal InternalStatementSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InternalStatementSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	internal InternalStatementSyntax(SyntaxKind kind, LexerMode mode)
		: base(kind, mode)
	{
	}

	internal InternalSyntaxNode GetTerminatingSemicolon()
	{
		InternalSyntaxNode lastTerminal = GetLastTerminal();
		if (lastTerminal.Kind != SyntaxKind.SemicolonToken)
		{
			return null;
		}
		return lastTerminal;
	}
}
